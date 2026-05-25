using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace GieudexPol.Tests
{
    public class ExchangeRateSyncServiceTests
    {
        [Fact]
        public async Task SyncNbpRatesAsync_ShouldCreateSourceCurrencyAndExchangeRate()
        {
            await using var context = CreateContext();
            var client = new FakeExternalExchangeRateClient
            {
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "C",
                        Number = "001/C/NBP/2026",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "EUR",
                                CurrencyName = "euro",
                                BuyPrice = 4.2010m,
                                SellPrice = 4.2850m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, client);

            var result = await service.SyncNbpRatesAsync(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(1);
            result.Skipped.Should().Be(0);
            result.TablesFetched.Should().Be(1);

            var rateSource = await context.RateSources.SingleAsync();
            rateSource.Code.Should().Be("NBP");
            rateSource.Name.Should().Be("Narodowy Bank Polski");

            var currency = await context.Currencies.SingleAsync();
            currency.Symbol.Should().Be("EUR");
            currency.Name.Should().Be("euro");

            var exchangeRate = await context.ExchangeRates.SingleAsync();
            exchangeRate.CurrencyId.Should().Be(currency.Id);
            exchangeRate.RateSourceId.Should().Be(rateSource.Id);
            exchangeRate.EffectiveDate.Should().Be(new DateTime(2026, 1, 2));
            exchangeRate.BuyPrice.Should().Be(4.2010m);
            exchangeRate.SellPrice.Should().Be(4.2850m);
        }

        [Fact]
        public async Task SyncNbpRatesAsync_ShouldSkipExistingRateForSameCurrencySourceAndDate()
        {
            await using var context = CreateContext();
            var currency = new Currency { Symbol = "EUR", Name = "euro", IsActive = true };
            var rateSource = new RateSource { Code = "NBP", Name = "Narodowy Bank Polski", IsActive = true };

            context.Currencies.Add(currency);
            context.RateSources.Add(rateSource);
            context.ExchangeRates.Add(new ExchangeRate
            {
                Currency = currency,
                RateSource = rateSource,
                EffectiveDate = new DateTime(2026, 1, 2),
                FetchedAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
                BuyPrice = 4.1000m,
                SellPrice = 4.2000m
            });
            await context.SaveChangesAsync();

            var client = new FakeExternalExchangeRateClient
            {
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "C",
                        Number = "001/C/NBP/2026",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "EUR",
                                CurrencyName = "euro",
                                BuyPrice = 4.2010m,
                                SellPrice = 4.2850m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, client);

            var result = await service.SyncNbpRatesAsync(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(0);
            result.Skipped.Should().Be(1);
            (await context.ExchangeRates.CountAsync()).Should().Be(1);

            var unchangedRate = await context.ExchangeRates.SingleAsync();
            unchangedRate.BuyPrice.Should().Be(4.1000m);
            unchangedRate.SellPrice.Should().Be(4.2000m);
        }

        [Fact]
        public async Task SyncNbpRatesAsync_ShouldSplitDateRangeUsingProviderMaxRangeDays()
        {
            await using var context = CreateContext();
            var client = new FakeExternalExchangeRateClient
            {
                MaxRangeDays = 2,
                TablesToReturn = []
            };
            var service = CreateService(context, client);

            var result = await service.SyncNbpRatesAsync(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 5));

            client.RequestedRanges.Should().BeEquivalentTo(
                [
                    (new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)),
                    (new DateTime(2026, 1, 3), new DateTime(2026, 1, 4)),
                    (new DateTime(2026, 1, 5), new DateTime(2026, 1, 5))
                ],
                options => options.WithStrictOrdering());
            result.ProcessedRanges.Should().HaveCount(3);
            result.Added.Should().Be(0);
        }

        [Fact]
        public async Task SyncRatesAsync_ShouldSelectEcbClientBySourceCode()
        {
            await using var context = CreateContext();
            var nbpClient = new FakeExternalExchangeRateClient();
            var ecbClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "ECB",
                SourceName = "European Central Bank",
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "ECB",
                        Number = "ECB/2026-01-02",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "USD",
                                CurrencyName = "USD",
                                BuyPrice = 3.8636m,
                                SellPrice = 3.8636m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, nbpClient, ecbClient);

            var result = await service.SyncRatesAsync(
                "ECB",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(1);
            nbpClient.RequestedRanges.Should().BeEmpty();
            ecbClient.RequestedRanges.Should().ContainSingle()
                .Which.Should().Be((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));

            var rateSource = await context.RateSources.SingleAsync();
            rateSource.Code.Should().Be("ECB");
            rateSource.Name.Should().Be("European Central Bank");
        }

        [Fact]
        public async Task SyncNbpRatesAsync_ShouldStillSelectNbpClient()
        {
            await using var context = CreateContext();
            var nbpClient = new FakeExternalExchangeRateClient();
            var ecbClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "ECB",
                SourceName = "European Central Bank"
            };
            var service = CreateService(context, nbpClient, ecbClient);

            await service.SyncNbpRatesAsync(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            nbpClient.RequestedRanges.Should().ContainSingle()
                .Which.Should().Be((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));
            ecbClient.RequestedRanges.Should().BeEmpty();
        }

        [Fact]
        public async Task SyncRatesAsync_ShouldSelectRiksbankClientBySourceCode()
        {
            await using var context = CreateContext();
            var nbpClient = new FakeExternalExchangeRateClient();
            var ecbClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "ECB",
                SourceName = "European Central Bank"
            };
            var riksbankClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "RIKSBANK",
                SourceName = "Sveriges Riksbank",
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "RIKSBANK",
                        Number = "RIKSBANK/2026-01-02",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "USD",
                                CurrencyName = "USD",
                                BuyPrice = 4.105263m,
                                SellPrice = 4.105263m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, nbpClient, ecbClient, riksbankClient);

            var result = await service.SyncRatesAsync(
                "RIKSBANK",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(1);
            nbpClient.RequestedRanges.Should().BeEmpty();
            ecbClient.RequestedRanges.Should().BeEmpty();
            riksbankClient.RequestedRanges.Should().ContainSingle()
                .Which.Should().Be((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));

            var rateSource = await context.RateSources.SingleAsync();
            rateSource.Code.Should().Be("RIKSBANK");
            rateSource.Name.Should().Be("Sveriges Riksbank");
        }

        [Fact]
        public async Task SyncRatesAsync_ShouldSelectBankOfEnglandClientBySourceCode()
        {
            await using var context = CreateContext();
            var nbpClient = new FakeExternalExchangeRateClient();
            var boeClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "BOE",
                SourceName = "Bank of England",
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "BOE",
                        Number = "BOE/2026-01-02",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "USD",
                                CurrencyName = "USD",
                                BuyPrice = 4.08m,
                                SellPrice = 4.08m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, nbpClient, boeClient);

            var result = await service.SyncRatesAsync(
                "BOE",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(1);
            nbpClient.RequestedRanges.Should().BeEmpty();
            boeClient.RequestedRanges.Should().ContainSingle()
                .Which.Should().Be((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));

            var rateSource = await context.RateSources.SingleAsync();
            rateSource.Code.Should().Be("BOE");
            rateSource.Name.Should().Be("Bank of England");
        }

        [Fact]
        public async Task SyncRatesAsync_ShouldSelectCnbClientBySourceCode()
        {
            await using var context = CreateContext();
            var nbpClient = new FakeExternalExchangeRateClient();
            var boeClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "BOE",
                SourceName = "Bank of England"
            };
            var cnbClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "CNB",
                SourceName = "Czech National Bank",
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "CNB",
                        Number = "CNB/2026-01-02",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "USD",
                                CurrencyName = "USD",
                                BuyPrice = 4.00m,
                                SellPrice = 4.00m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, nbpClient, boeClient, cnbClient);

            var result = await service.SyncRatesAsync(
                "CNB",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(1);
            nbpClient.RequestedRanges.Should().BeEmpty();
            boeClient.RequestedRanges.Should().BeEmpty();
            cnbClient.RequestedRanges.Should().ContainSingle()
                .Which.Should().Be((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));

            var rateSource = await context.RateSources.SingleAsync();
            rateSource.Code.Should().Be("CNB");
            rateSource.Name.Should().Be("Czech National Bank");
        }

        [Fact]
        public async Task SyncRatesAsync_ShouldSelectNorgesClientBySourceCode()
        {
            await using var context = CreateContext();
            var nbpClient = new FakeExternalExchangeRateClient();
            var cnbClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "CNB",
                SourceName = "Czech National Bank"
            };
            var norgesClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "NORGES",
                SourceName = "Norges Bank",
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "NORGES",
                        Number = "NORGES/2026-01-02",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "USD",
                                CurrencyName = "USD",
                                BuyPrice = 4.04m,
                                SellPrice = 4.04m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, nbpClient, cnbClient, norgesClient);

            var result = await service.SyncRatesAsync(
                "NORGES",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(1);
            nbpClient.RequestedRanges.Should().BeEmpty();
            cnbClient.RequestedRanges.Should().BeEmpty();
            norgesClient.RequestedRanges.Should().ContainSingle()
                .Which.Should().Be((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));

            var rateSource = await context.RateSources.SingleAsync();
            rateSource.Code.Should().Be("NORGES");
            rateSource.Name.Should().Be("Norges Bank");
        }

        [Fact]
        public async Task SyncRatesAsync_ShouldSelectBnrClientBySourceCode()
        {
            await using var context = CreateContext();
            var norgesClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "NORGES",
                SourceName = "Norges Bank"
            };
            var bnrClient = new FakeExternalExchangeRateClient
            {
                SourceCode = "BNR",
                SourceName = "National Bank of Romania",
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "BNR",
                        Number = "BNR/2026-01-02",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "USD",
                                CurrencyName = "USD",
                                BuyPrice = 4.26m,
                                SellPrice = 4.26m
                            }
                        ]
                    }
                ]
            };
            var service = CreateService(context, norgesClient, bnrClient);

            var result = await service.SyncRatesAsync(
                "BNR",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2));

            result.Added.Should().Be(1);
            norgesClient.RequestedRanges.Should().BeEmpty();
            bnrClient.RequestedRanges.Should().ContainSingle()
                .Which.Should().Be((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));

            var rateSource = await context.RateSources.SingleAsync();
            rateSource.Code.Should().Be("BNR");
            rateSource.Name.Should().Be("National Bank of Romania");
        }

        [Fact]
        public async Task SyncRatesAsync_ShouldSerializeConcurrentSyncsForTheSameSource()
        {
            var databaseName = Guid.NewGuid().ToString();
            var databaseRoot = new InMemoryDatabaseRoot();
            await using var firstContext = CreateContext(databaseName, databaseRoot);
            await using var secondContext = CreateContext(databaseName, databaseRoot);

            var firstClient = CreateBoeClientWithDelay();
            var secondClient = CreateBoeClientWithDelay();
            var firstService = CreateService(firstContext, firstClient);
            var secondService = CreateService(secondContext, secondClient);

            var results = await Task.WhenAll(
                firstService.SyncRatesAsync("BOE", new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)),
                secondService.SyncRatesAsync("BOE", new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));

            await using var verificationContext = CreateContext(databaseName, databaseRoot);
            (await verificationContext.RateSources.CountAsync(source => source.Code == "BOE")).Should().Be(1);
            (await verificationContext.ExchangeRates.CountAsync()).Should().Be(1);
            results.Sum(result => result.Added).Should().Be(1);
            results.Sum(result => result.Skipped).Should().Be(1);
        }

        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static ApplicationDbContext CreateContext(string databaseName, InMemoryDatabaseRoot databaseRoot)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName, databaseRoot)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static FakeExternalExchangeRateClient CreateBoeClientWithDelay()
        {
            return new FakeExternalExchangeRateClient
            {
                SourceCode = "BOE",
                SourceName = "Bank of England",
                RequestDelay = TimeSpan.FromMilliseconds(50),
                TablesToReturn =
                [
                    new ExternalExchangeRateTableDto
                    {
                        Table = "BOE",
                        Number = "BOE/2026-01-02",
                        EffectiveDate = new DateTime(2026, 1, 2),
                        Rates =
                        [
                            new ExternalExchangeRateItemDto
                            {
                                CurrencyCode = "USD",
                                CurrencyName = "USD",
                                BuyPrice = 4.08m,
                                SellPrice = 4.08m
                            }
                        ]
                    }
                ]
            };
        }

        private static ExchangeRateSyncService CreateService(
            ApplicationDbContext context,
            params IExternalExchangeRateClient[] clients)
        {
            return new ExchangeRateSyncService(
                context,
                clients,
                NullLogger<ExchangeRateSyncService>.Instance);
        }

        private sealed class FakeExternalExchangeRateClient : IExternalExchangeRateClient
        {
            public string SourceCode { get; init; } = "NBP";
            public string SourceName { get; init; } = "Narodowy Bank Polski";
            public int MaxRangeDays { get; init; } = 93;
            public IReadOnlyList<ExternalExchangeRateTableDto> TablesToReturn { get; init; } = [];
            public TimeSpan RequestDelay { get; init; }
            public List<(DateTime From, DateTime To)> RequestedRanges { get; } = [];

            public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
                DateTime from,
                DateTime to,
                CancellationToken cancellationToken = default)
            {
                RequestedRanges.Add((from.Date, to.Date));
                if (RequestDelay > TimeSpan.Zero)
                {
                    await Task.Delay(RequestDelay, cancellationToken);
                }

                return TablesToReturn;
            }
        }
    }
}
