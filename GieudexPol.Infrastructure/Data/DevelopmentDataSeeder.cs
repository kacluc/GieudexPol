using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using AuthUser = GieudexPol.Domain.Auth.User;

namespace GieudexPol.Infrastructure.Data
{
    public static class DevelopmentDataSeeder
    {
        private const string DevelopmentSourceCodeA = DevelopmentIdentity.RateSourceCode;
        private const string DevelopmentSourceCodeB = DevelopmentIdentity.RateSourceCodeB;
        private const string DevelopmentSourceNameA = "Development Mock Bank A";
        private const string DevelopmentSourceNameB = "Development Mock Bank B";
        public const string DevelopmentUserEmail = DevelopmentIdentity.UserEmail;
        public const string DevelopmentUserPassword = "DevPassword123!";
        public const string SecondAdminEmail = "admin2@gieudexpol.local";
        public const string SecondAdminPassword = "AdminPassword123!";
        public const string DemoUserPassword = "DemoPassword123!";
        private static readonly Guid DevelopmentUserAuthId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid DevelopmentTransferFeeId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        private static readonly Guid DevelopmentDepositFeeId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
        private static readonly Guid DevelopmentWithdrawalFeeId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
        private static readonly Guid DevelopmentOrderBookFeeId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        private static readonly Guid DevelopmentInstantExchangeFeeId = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
        private const string PlatformTreasuryUsername = "system_platform_treasury";
        private static readonly UserSeed[] SeedUsers =
        [
            new(
                DevelopmentUserAuthId,
                DevelopmentUserEmail,
                "Development User",
                DevelopmentUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 10_000m,
                    ["EUR"] = 1_000m,
                    ["USD"] = 1_000m,
                    ["CHF"] = 500m,
                    ["GBP"] = 500m
                },
                "Admin"),
            new(
                new Guid("12121212-1212-1212-1212-121212121212"),
                SecondAdminEmail,
                "Development Admin 2",
                SecondAdminPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 50_000m,
                    ["EUR"] = 5_000m,
                    ["USD"] = 5_000m,
                    ["CHF"] = 2_500m,
                    ["GBP"] = 2_500m
                },
                "Admin"),
            new(
                new Guid("22222222-2222-2222-2222-222222222222"),
                "zbigniew.stonoga@gieudexpol.local",
                "Zbigniew Stonoga",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 420_000m,
                    ["EUR"] = 18_000m,
                    ["USD"] = 12_500m
                }),
            new(
                new Guid("33333333-3333-3333-3333-333333333333"),
                "adam.malysz@gieudexpol.local",
                "Adam Małysz",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 615_000m,
                    ["EUR"] = 32_000m,
                    ["CHF"] = 21_000m
                }),
            new(
                new Guid("44444444-4444-4444-4444-444444444444"),
                "robert.kubica@gieudexpol.local",
                "Robert Kubica",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 940_000m,
                    ["EUR"] = 75_000m,
                    ["USD"] = 56_000m
                }),
            new(
                new Guid("55555555-5555-5555-5555-555555555555"),
                "robert.lewandowski@gieudexpol.local",
                "Robert Lewandowski",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 1_800_000m,
                    ["EUR"] = 180_000m,
                    ["GBP"] = 90_000m
                }),
            new(
                new Guid("66666666-6666-6666-6666-666666666666"),
                "lukasz.stanislawowski@gieudexpol.local",
                "Łukasz Stanisławowski",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 280_000m,
                    ["USD"] = 6_666_666_666m,
                    ["CHF"] = 7_000m
                }),
            new(
                new Guid("77777777-7777-7777-7777-777777777777"),
                "zenek.martyniuk@gieudexpol.local",
                "Zenek Martyniuk",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 760_000m,
                    ["EUR"] = 45_000m,
                    ["GBP"] = 22_000m
                }),
            new(
                new Guid("88888888-8888-8888-8888-888888888888"),
                "maryla.rodowicz@gieudexpol.local",
                "Maryla Rodowicz",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 880_000m,
                    ["EUR"] = 62_000m,
                    ["USD"] = 35_000m
                }),
            new(
                new Guid("99999999-9999-9999-9999-999999999999"),
                "teddy.kaczynski@gieudexpol.local",
                "Teddy Kaczynski",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 350_000m,
                    ["USD"] = 40_000m,
                    ["CHF"] = 12_000m
                }),
            new(
                new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "janusz.kowalski@gieudexpol.local",
                "Janusz Kowalski",
                DemoUserPassword,
                new Dictionary<string, decimal>
                {
                    ["PLN"] = 225_000m,
                    ["EUR"] = 9_000m,
                    ["USD"] = 8_000m
                })
        ];

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(DevelopmentDataSeeder));

            if (context.Database.IsRelational() &&
                !await context.Database.CanConnectAsync())
            {
                logger.LogWarning("Development seed skipped because the database is not available.");
                return;
            }

            var addedCurrencies = await SeedCurrenciesAsync(context);
            var addedTradingPairs = await SeedTradingPairsAsync(context);
            var addedUsers = await SeedUsersAsync(context);
            var addedWallets = await SeedWalletsAsync(context);
            var addedTransactionFees = await SeedTransactionFeesAsync(context);
            var rateSourceA = await SeedRateSourceAsync(
                context,
                DevelopmentSourceCodeA,
                DevelopmentSourceNameA);
            var rateSourceB = await SeedRateSourceAsync(
                context,
                DevelopmentSourceCodeB,
                DevelopmentSourceNameB);
            var addedSystemAccounts = await SeedSystemAccountsCoreAsync(
                context,
                initializeLiquidity: true);
            var addedRatesA = await SeedExchangeRatesAsync(
                context,
                rateSourceA,
                randomSeed: 12345,
                priceMultiplier: 1m);
            var addedRatesB = await SeedExchangeRatesAsync(
                context,
                rateSourceB,
                randomSeed: 67890,
                priceMultiplier: 1.018m);
            var addedRates = addedRatesA + addedRatesB;

            logger.LogInformation(
                "Development seed completed. Added {CurrencyCount} currencies, {TradingPairCount} trading pairs, {UserCount} users, {WalletCount} wallets, {TransactionFeeCount} transaction fees, {SystemAccountCount} system accounts and {RateCount} exchange rates.",
                addedCurrencies,
                addedTradingPairs,
                addedUsers,
                addedWallets,
                addedTransactionFees,
                addedSystemAccounts,
                addedRates);
        }

        public static async Task<int> SeedSystemAccountsAsync(
            IServiceProvider serviceProvider,
            bool initializeLiquidity)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            return await SeedSystemAccountsCoreAsync(
                context,
                initializeLiquidity);
        }

        private static async Task<int> SeedUsersAsync(ApplicationDbContext context)
        {
            var seedEmails = SeedUsers.Select(seed => seed.Email).ToList();
            var existingUsers = await context.Users
                .Where(user => seedEmails.Contains(user.Username))
                .ToListAsync();
            var existingUsersByEmail = existingUsers.ToDictionary(user => user.Username);
            var passwordHasher = new PasswordHasher<AuthUser>();
            var usersToAdd = new List<User>();

            foreach (var seed in SeedUsers)
            {
                if (existingUsersByEmail.TryGetValue(seed.Email, out var existingUser))
                {
                    existingUser.DisplayName = seed.DisplayName;
                    existingUser.Role = seed.Role;
                    existingUser.AccountType = ResolveAccountType(seed.Role);
                    continue;
                }

                var authUser = new AuthUser(seed.AuthId, seed.Email, seed.Password);
                usersToAdd.Add(new User
                {
                    AuthId = seed.AuthId,
                    Username = seed.Email,
                    DisplayName = seed.DisplayName,
                    PasswordHash = passwordHasher.HashPassword(authUser, seed.Password),
                    Role = seed.Role,
                    AccountType = ResolveAccountType(seed.Role)
                });
            }

            if (usersToAdd.Count > 0)
            {
                await context.Users.AddRangeAsync(usersToAdd);
            }

            await context.SaveChangesAsync();
            return usersToAdd.Count;
        }

        private static async Task<int> SeedCurrenciesAsync(ApplicationDbContext context)
        {
            var seedCurrencies = new[]
            {
                new Currency { Symbol = "PLN", Name = "Polish Zloty", IsActive = true },
                new Currency { Symbol = "EUR", Name = "Euro", IsActive = true },
                new Currency { Symbol = "USD", Name = "US Dollar", IsActive = true },
                new Currency { Symbol = "CHF", Name = "Swiss Franc", IsActive = true },
                new Currency { Symbol = "GBP", Name = "British Pound", IsActive = true },
                new Currency { Symbol = "HUF", Name = "Hungarian Forint", IsActive = true },
                new Currency { Symbol = "CZK", Name = "Czech Koruna", IsActive = true },
                new Currency { Symbol = "DKK", Name = "Danish Krone", IsActive = true },
                new Currency { Symbol = "SEK", Name = "Swedish Krona", IsActive = true },
                new Currency { Symbol = "NOK", Name = "Norwegian Krone", IsActive = true },
                new Currency { Symbol = "RON", Name = "Romanian Leu", IsActive = true },
                new Currency { Symbol = "TRY", Name = "Turkish Lira", IsActive = true },
                new Currency { Symbol = "UAH", Name = "Ukrainian Hryvnia", IsActive = true },
                new Currency { Symbol = "AUD", Name = "Australian Dollar", IsActive = true },
                new Currency { Symbol = "CAD", Name = "Canadian Dollar", IsActive = true },
                new Currency { Symbol = "JPY", Name = "Japanese Yen", IsActive = true },
                new Currency { Symbol = "KRW", Name = "South Korean Won", IsActive = true }
            };

            var existingSymbols = await context.Currencies
                .Where(currency => seedCurrencies.Select(seed => seed.Symbol).Contains(currency.Symbol))
                .Select(currency => currency.Symbol)
                .ToListAsync();

            var currenciesToAdd = seedCurrencies
                .Where(currency => !existingSymbols.Contains(currency.Symbol))
                .ToList();

            if (currenciesToAdd.Count == 0)
            {
                return 0;
            }

            await context.Currencies.AddRangeAsync(currenciesToAdd);
            await context.SaveChangesAsync();

            return currenciesToAdd.Count;
        }

        private static async Task<int> SeedTradingPairsAsync(ApplicationDbContext context)
        {
            var currencies = await context.Currencies
                .Where(currency => currency.IsActive)
                .ToListAsync();
            var pln = currencies.SingleOrDefault(currency => currency.Symbol == "PLN");
            if (pln == null)
            {
                return 0;
            }

            var existingPairs = await context.TradingPairs
                .Where(pair => pair.QuoteCurrencyId == pln.Id)
                .ToListAsync();

            var pairsToAdd = new List<TradingPair>();
            foreach (var currency in currencies.Where(currency => currency.Id != pln.Id))
            {
                var existingPair = existingPairs.SingleOrDefault(pair =>
                    pair.BaseCurrencyId == currency.Id);
                if (existingPair != null)
                {
                    existingPair.IsActive = true;
                    existingPair.TickSize = 0.0001m;
                    continue;
                }

                pairsToAdd.Add(new TradingPair
                {
                    BaseCurrencyId = currency.Id,
                    QuoteCurrencyId = pln.Id,
                    IsActive = true,
                    TickSize = 0.0001m
                });
            }

            if (pairsToAdd.Count > 0)
            {
                await context.TradingPairs.AddRangeAsync(pairsToAdd);
            }

            await context.SaveChangesAsync();
            return pairsToAdd.Count;
        }

        private static async Task<int> SeedWalletsAsync(ApplicationDbContext context)
        {
            var seedEmails = SeedUsers.Select(seed => seed.Email).ToList();
            var users = await context.Users
                .Where(user => seedEmails.Contains(user.Username))
                .ToDictionaryAsync(user => user.Username);
            var symbols = SeedUsers
                .SelectMany(seed => seed.Balances.Keys)
                .Distinct()
                .ToList();
            var currencies = await context.Currencies
                .Where(currency => symbols.Contains(currency.Symbol))
                .ToDictionaryAsync(currency => currency.Symbol);
            var userIds = users.Values.Select(user => user.Id).ToList();
            var existingWallets = await context.Wallets
                .Where(wallet => userIds.Contains(wallet.UserId))
                .ToListAsync();
            var existingWalletsByKey = existingWallets
                .ToDictionary(wallet => (wallet.UserId, wallet.CurrencyId));
            var walletsToAdd = new List<Wallet>();
            var updatedWallets = 0;

            foreach (var seed in SeedUsers)
            {
                if (!users.TryGetValue(seed.Email, out var user))
                {
                    continue;
                }

                foreach (var (symbol, balance) in seed.Balances)
                {
                    if (!currencies.TryGetValue(symbol, out var currency))
                    {
                        continue;
                    }

                    if (existingWalletsByKey.TryGetValue((user.Id, currency.Id), out var existingWallet))
                    {
                        if (seed.Email == "lukasz.stanislawowski@gieudexpol.local" &&
                            symbol == "USD" &&
                            existingWallet.Balance == 15_500m)
                        {
                            existingWallet.Balance = balance;
                            updatedWallets++;
                        }

                        continue;
                    }

                    walletsToAdd.Add(new Wallet
                    {
                        UserId = user.Id,
                        CurrencyId = currency.Id,
                        Balance = balance
                    });
                }
            }

            if (walletsToAdd.Count == 0 && updatedWallets == 0)
            {
                return 0;
            }

            await context.Wallets.AddRangeAsync(walletsToAdd);
            await context.SaveChangesAsync();

            return walletsToAdd.Count + updatedWallets;
        }

        private static async Task<RateSource> SeedRateSourceAsync(
            ApplicationDbContext context,
            string code,
            string name)
        {
            var rateSource = await context.RateSources
                .FirstOrDefaultAsync(source => source.Code == code);

            if (rateSource != null)
            {
                var requiresUpdate = rateSource.Name != name || !rateSource.IsActive;
                rateSource.Name = name;
                rateSource.IsActive = true;

                if (requiresUpdate)
                {
                    await context.SaveChangesAsync();
                }

                return rateSource;
            }

            rateSource = new RateSource
            {
                Code = code,
                Name = name,
                IsActive = true
            };

            await context.RateSources.AddAsync(rateSource);
            await context.SaveChangesAsync();

            return rateSource;
        }

        private static async Task<int> SeedTransactionFeesAsync(ApplicationDbContext context)
        {
            var definitions = new[]
            {
                new TransactionFee
                {
                    Id = DevelopmentTransferFeeId,
                    Type = "Transfer"
                },
                new TransactionFee
                {
                    Id = DevelopmentDepositFeeId,
                    Type = "Deposit"
                },
                new TransactionFee
                {
                    Id = DevelopmentWithdrawalFeeId,
                    Type = "Withdrawal"
                },
                new TransactionFee
                {
                    Id = DevelopmentOrderBookFeeId,
                    Type = "OrderBook"
                },
                new TransactionFee
                {
                    Id = DevelopmentInstantExchangeFeeId,
                    Type = "InstantExchange"
                }
            };
            var types = definitions.Select(definition => definition.Type).ToList();
            var existingFees = await context.Set<TransactionFee>()
                .Where(fee => types.Contains(fee.Type))
                .ToDictionaryAsync(fee => fee.Type);
            var addedCount = 0;

            foreach (var definition in definitions)
            {
                if (!existingFees.TryGetValue(definition.Type, out var fee))
                {
                    fee = definition;
                    await context.Set<TransactionFee>().AddAsync(fee);
                    addedCount++;
                }

                fee.FeePercentage = 0.5m;
                fee.FlatFee = 10m;
                fee.IsActive = true;
            }

            await context.SaveChangesAsync();
            return addedCount;
        }

        private static async Task<int> SeedSystemAccountsCoreAsync(
            ApplicationDbContext context,
            bool initializeLiquidity)
        {
            var currencies = await context.Currencies
                .Where(currency => currency.IsActive)
                .ToListAsync();
            var rateSources = await context.RateSources
                .Where(source => source.IsActive)
                .ToListAsync();
            var usersToAdd = new List<User>();
            var addedAccounts = 0;

            var treasury = await context.Users.SingleOrDefaultAsync(user =>
                user.AccountType == AccountType.PlatformTreasury ||
                user.Username == PlatformTreasuryUsername);
            if (treasury == null)
            {
                treasury = CreateSystemUser(
                    PlatformTreasuryUsername,
                    "Platform Treasury",
                    AccountType.PlatformTreasury);
                usersToAdd.Add(treasury);
                addedAccounts++;
            }
            else
            {
                treasury.AccountType = AccountType.PlatformTreasury;
                treasury.Role = "System";
            }

            foreach (var source in rateSources)
            {
                var username = GetRateSourceSystemUsername(source.Code);
                var systemUser = source.SystemUserId.HasValue
                    ? await context.Users.FindAsync(source.SystemUserId.Value)
                    : await context.Users.SingleOrDefaultAsync(user =>
                        user.Username == username);

                if (systemUser == null)
                {
                    systemUser = CreateSystemUser(
                        username,
                        $"System liquidity: {source.Code}",
                        AccountType.RateSourceSystem);
                    usersToAdd.Add(systemUser);
                    addedAccounts++;
                }
                else
                {
                    systemUser.AccountType = AccountType.RateSourceSystem;
                    systemUser.Role = "System";
                }

                source.SystemUser = systemUser;
            }

            if (usersToAdd.Count > 0)
            {
                await context.Users.AddRangeAsync(usersToAdd);
            }

            await context.SaveChangesAsync();

            var systemUsers = await context.Users
                .Where(user =>
                    user.AccountType == AccountType.RateSourceSystem ||
                    user.AccountType == AccountType.PlatformTreasury)
                .ToListAsync();
            var userIds = systemUsers.Select(user => user.Id).ToList();
            var existingWallets = await context.Wallets
                .Where(wallet => userIds.Contains(wallet.UserId))
                .Select(wallet => new { wallet.UserId, wallet.CurrencyId })
                .ToListAsync();
            var walletKeys = existingWallets
                .Select(wallet => (wallet.UserId, wallet.CurrencyId))
                .ToHashSet();
            var walletsToAdd = new List<Wallet>();

            foreach (var user in systemUsers)
            {
                foreach (var currency in currencies)
                {
                    if (walletKeys.Contains((user.Id, currency.Id)))
                    {
                        continue;
                    }

                    walletsToAdd.Add(new Wallet
                    {
                        UserId = user.Id,
                        CurrencyId = currency.Id,
                        Balance = user.AccountType == AccountType.PlatformTreasury ||
                                  !initializeLiquidity
                            ? 0m
                            : GetSystemLiquidityBalance(currency.Symbol),
                        ReservedBalance = 0m
                    });
                }
            }

            if (walletsToAdd.Count > 0)
            {
                await context.Wallets.AddRangeAsync(walletsToAdd);
                await context.SaveChangesAsync();
            }

            return addedAccounts;
        }

        private static User CreateSystemUser(
            string username,
            string displayName,
            AccountType accountType)
        {
            return new User
            {
                AuthId = CreateDeterministicGuid(username),
                Username = username,
                DisplayName = displayName,
                PasswordHash = "!SYSTEM_ACCOUNT_NO_LOGIN!",
                Role = "System",
                AccountType = accountType
            };
        }

        private static Guid CreateDeterministicGuid(string value)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return new Guid(hash[..16]);
        }

        private static string GetRateSourceSystemUsername(string sourceCode)
        {
            var normalized = new string(sourceCode
                .Trim()
                .ToLowerInvariant()
                .Select(character => char.IsLetterOrDigit(character) ? character : '_')
                .ToArray());
            return "system_" + normalized;
        }

        private static decimal GetSystemLiquidityBalance(string symbol)
        {
            return symbol switch
            {
                "PLN" => 1_000_000m,
                "EUR" or "USD" => 400_000m,
                "GBP" or "CHF" => 200_000m,
                "JPY" or "KRW" => 50_000_000m,
                _ => 500_000m
            };
        }

        private static AccountType ResolveAccountType(string role)
        {
            return role switch
            {
                "SuperAdmin" => AccountType.SuperAdminUser,
                "Admin" => AccountType.AdminUser,
                _ => AccountType.RegularUser
            };
        }

        private static async Task<int> SeedExchangeRatesAsync(
            ApplicationDbContext context,
            RateSource rateSource,
            int randomSeed,
            decimal priceMultiplier)
        {
            var startDate = new DateTime(2026, 1, 1);
            var endDate = DateTime.Today.AddDays(-1);

            var currentOrFutureRates = await context.ExchangeRates
                .Where(rate =>
                    rate.RateSourceId == rateSource.Id &&
                    rate.EffectiveDate >= DateTime.Today)
                .ToListAsync();
            if (currentOrFutureRates.Count > 0)
            {
                context.ExchangeRates.RemoveRange(currentOrFutureRates);
                await context.SaveChangesAsync();
            }

            if (endDate < startDate)
            {
                return 0;
            }

            var currencyModels = new[]
            {
                new CurrencyRateSeed("EUR", 4.30m, 0.00035m, 0.045m),
                new CurrencyRateSeed("USD", 3.95m, -0.00015m, 0.040m),
                new CurrencyRateSeed("CHF", 4.55m, 0.00025m, 0.055m),
                new CurrencyRateSeed("GBP", 5.05m, 0.00020m, 0.065m),
                new CurrencyRateSeed("HUF", 1.08m, 0.00010m, 0.020m),
                new CurrencyRateSeed("CZK", 0.17m, 0.00002m, 0.006m),
                new CurrencyRateSeed("DKK", 0.58m, 0.00004m, 0.012m),
                new CurrencyRateSeed("SEK", 0.39m, -0.00002m, 0.010m),
                new CurrencyRateSeed("NOK", 0.37m, 0.00001m, 0.010m),
                new CurrencyRateSeed("RON", 0.86m, 0.00003m, 0.018m),
                new CurrencyRateSeed("TRY", 0.12m, -0.00004m, 0.008m),
                new CurrencyRateSeed("UAH", 0.095m, -0.00001m, 0.007m),
                new CurrencyRateSeed("AUD", 2.55m, 0.00012m, 0.035m),
                new CurrencyRateSeed("CAD", 2.90m, 0.00010m, 0.035m),
                new CurrencyRateSeed("JPY", 2.65m, -0.00008m, 0.030m),
                new CurrencyRateSeed("KRW", 0.28m, 0.00001m, 0.009m)
            };

            var symbols = currencyModels.Select(model => model.Symbol).ToList();
            var currencies = await context.Currencies
                .Where(currency => symbols.Contains(currency.Symbol))
                .ToDictionaryAsync(currency => currency.Symbol);

            var existingRateKeys = await context.ExchangeRates
                .Where(rate =>
                    rate.RateSourceId == rateSource.Id &&
                    rate.EffectiveDate >= startDate &&
                    rate.EffectiveDate <= endDate)
                .Select(rate => new { rate.CurrencyId, rate.EffectiveDate })
                .ToListAsync();

            var existingRates = existingRateKeys
                .Select(rate => (rate.CurrencyId, Date: rate.EffectiveDate.Date))
                .ToHashSet();

            var random = new Random(randomSeed);
            var ratesToAdd = new List<ExchangeRate>();

            foreach (var currencyModel in currencyModels)
            {
                if (!currencies.TryGetValue(currencyModel.Symbol, out var currency))
                {
                    continue;
                }

                var adjustedStartMidPrice = currencyModel.StartMidPrice * priceMultiplier;
                var adjustedDailyTrend = currencyModel.DailyTrend * priceMultiplier;
                var adjustedBaseSpread = currencyModel.BaseSpread * priceMultiplier;
                var midPrice = adjustedStartMidPrice;
                var businessDayIndex = 0;

                foreach (var date in EachBusinessDay(startDate, endDate))
                {
                    var wave =
                        (decimal)Math.Sin(businessDayIndex / 12.0) *
                        0.012m *
                        priceMultiplier;
                    var randomMove =
                        ((decimal)random.NextDouble() - 0.5m) *
                        0.018m *
                        priceMultiplier;

                    midPrice += adjustedDailyTrend + wave / 30m + randomMove;
                    midPrice = Math.Max(midPrice, adjustedStartMidPrice - 0.25m * priceMultiplier);
                    midPrice = Math.Min(midPrice, adjustedStartMidPrice + 0.25m * priceMultiplier);

                    var spreadJitter =
                        ((decimal)random.NextDouble() - 0.5m) *
                        0.010m *
                        priceMultiplier;
                    var spread = Math.Max(
                        0.030m * priceMultiplier,
                        adjustedBaseSpread + spreadJitter);
                    var buyPrice = Math.Round(midPrice - spread / 2m, 4);
                    var sellPrice = Math.Round(midPrice + spread / 2m, 4);

                    if (!existingRates.Contains((currency.Id, date.Date)))
                    {
                        ratesToAdd.Add(new ExchangeRate
                        {
                            CurrencyId = currency.Id,
                            RateSourceId = rateSource.Id,
                            EffectiveDate = date.Date,
                            FetchedAt = date.Date.AddHours(16),
                            BuyPrice = buyPrice,
                            SellPrice = sellPrice,
                            MidPrice = Math.Round(midPrice, 4)
                        });
                    }

                    businessDayIndex++;
                }
            }

            if (ratesToAdd.Count == 0)
            {
                return 0;
            }

            await context.ExchangeRates.AddRangeAsync(ratesToAdd);
            await context.SaveChangesAsync();

            return ratesToAdd.Count;
        }

        private static IEnumerable<DateTime> EachBusinessDay(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                {
                    yield return date;
                }
            }
        }

        private sealed record CurrencyRateSeed(
            string Symbol,
            decimal StartMidPrice,
            decimal DailyTrend,
            decimal BaseSpread);

        private sealed record UserSeed(
            Guid AuthId,
            string Email,
            string DisplayName,
            string Password,
            IReadOnlyDictionary<string, decimal> Balances,
            string Role = "User");
    }
}
