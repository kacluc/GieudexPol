using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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

        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static ExchangeRateSyncService CreateService(
            ApplicationDbContext context,
            IExternalExchangeRateClient client)
        {
            return new ExchangeRateSyncService(
                context,
                client,
                NullLogger<ExchangeRateSyncService>.Instance);
        }

        private sealed class FakeExternalExchangeRateClient : IExternalExchangeRateClient
        {
            public string SourceCode { get; init; } = "NBP";
            public string SourceName { get; init; } = "Narodowy Bank Polski";
            public int MaxRangeDays { get; init; } = 93;
            public IReadOnlyList<ExternalExchangeRateTableDto> TablesToReturn { get; init; } = [];
            public List<(DateTime From, DateTime To)> RequestedRanges { get; } = [];

            public Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
                DateTime from,
                DateTime to,
                CancellationToken cancellationToken = default)
            {
                RequestedRanges.Add((from.Date, to.Date));
                return Task.FromResult(TablesToReturn);
            }
        }
    }
}
