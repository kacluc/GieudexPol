using GieudexPol.Application.DTOs;
using GieudexPol.Application.Exceptions;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class AdminTestExchangeRateService : IAdminTestExchangeRateService
    {
        private readonly ApplicationDbContext _context;

        public AdminTestExchangeRateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AdminTestExchangeRateDto>> GetRatesAsync(
            int? currencyId,
            string? currencyCode,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken = default)
        {
            var source = await GetDevelopmentSourceAsync(cancellationToken);
            ValidateDateRange(dateFrom, dateTo);

            var query = _context.ExchangeRates
                .AsNoTracking()
                .Include(rate => rate.Currency)
                .Include(rate => rate.RateSource)
                .Where(rate => rate.RateSourceId == source.Id);

            if (currencyId.HasValue)
            {
                query = query.Where(rate => rate.CurrencyId == currencyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(currencyCode))
            {
                var normalizedCode = currencyCode.Trim().ToUpperInvariant();
                query = query.Where(rate => rate.Currency.Symbol == normalizedCode);
            }

            if (dateFrom.HasValue)
            {
                var from = dateFrom.Value.Date;
                query = query.Where(rate => rate.EffectiveDate >= from);
            }

            if (dateTo.HasValue)
            {
                var toExclusive = dateTo.Value.Date.AddDays(1);
                query = query.Where(rate => rate.EffectiveDate < toExclusive);
            }

            var rates = await query
                .OrderByDescending(rate => rate.EffectiveDate)
                .ThenBy(rate => rate.Currency.Symbol)
                .ToListAsync(cancellationToken);

            return rates.Select(ToDto).ToList();
        }

        public async Task<AdminTestExchangeRateDto?> GetRateAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var source = await GetDevelopmentSourceAsync(cancellationToken);
            var rate = await _context.ExchangeRates
                .AsNoTracking()
                .Include(item => item.Currency)
                .Include(item => item.RateSource)
                .FirstOrDefaultAsync(
                    item => item.Id == id && item.RateSourceId == source.Id,
                    cancellationToken);

            return rate == null ? null : ToDto(rate);
        }

        public async Task<AdminTestExchangeRateDto> CreateRateAsync(
            CreateTestExchangeRateDto request,
            CancellationToken cancellationToken = default)
        {
            ValidatePrices(request.EffectiveDate, request.BuyPrice, request.SellPrice, request.MidPrice);
            var source = await GetDevelopmentSourceAsync(cancellationToken);
            var currency = await ResolveCurrencyAsync(
                request.CurrencyId,
                request.CurrencyCode,
                cancellationToken);
            var effectiveDate = request.EffectiveDate.Date;

            if (await HasDuplicateAsync(
                    currency.Id,
                    source.Id,
                    effectiveDate,
                    excludedRateId: null,
                    cancellationToken))
            {
                throw new TestExchangeRateConflictException(
                    $"Testowy kurs {currency.Symbol} dla daty {effectiveDate:yyyy-MM-dd} juz istnieje.");
            }

            var rate = new ExchangeRate
            {
                CurrencyId = currency.Id,
                Currency = currency,
                RateSourceId = source.Id,
                RateSource = source,
                EffectiveDate = effectiveDate,
                BuyPrice = request.BuyPrice,
                SellPrice = request.SellPrice,
                MidPrice = ResolveMidPrice(request.BuyPrice, request.SellPrice, request.MidPrice),
                FetchedAt = DateTime.UtcNow
            };

            _context.ExchangeRates.Add(rate);
            await SaveChangesAsync(currency.Symbol, effectiveDate, cancellationToken);

            return ToDto(rate);
        }

        public async Task<AdminTestExchangeRateDto?> UpdateRateAsync(
            int id,
            UpdateTestExchangeRateDto request,
            CancellationToken cancellationToken = default)
        {
            ValidatePrices(request.EffectiveDate, request.BuyPrice, request.SellPrice, request.MidPrice);
            var source = await GetDevelopmentSourceAsync(cancellationToken);
            var rate = await _context.ExchangeRates
                .Include(item => item.Currency)
                .Include(item => item.RateSource)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (rate == null)
            {
                return null;
            }

            EnsureDevelopmentRate(rate, source);
            var effectiveDate = request.EffectiveDate.Date;

            if (await HasDuplicateAsync(
                    rate.CurrencyId,
                    source.Id,
                    effectiveDate,
                    id,
                    cancellationToken))
            {
                throw new TestExchangeRateConflictException(
                    $"Testowy kurs {rate.Currency.Symbol} dla daty {effectiveDate:yyyy-MM-dd} juz istnieje.");
            }

            rate.EffectiveDate = effectiveDate;
            rate.BuyPrice = request.BuyPrice;
            rate.SellPrice = request.SellPrice;
            rate.MidPrice = ResolveMidPrice(request.BuyPrice, request.SellPrice, request.MidPrice);
            rate.FetchedAt = DateTime.UtcNow;

            await SaveChangesAsync(rate.Currency.Symbol, effectiveDate, cancellationToken);
            return ToDto(rate);
        }

        public async Task<bool> DeleteRateAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var source = await GetDevelopmentSourceAsync(cancellationToken);
            var rate = await _context.ExchangeRates
                .Include(item => item.RateSource)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (rate == null)
            {
                return false;
            }

            EnsureDevelopmentRate(rate, source);
            _context.ExchangeRates.Remove(rate);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task<RateSource> GetDevelopmentSourceAsync(
            CancellationToken cancellationToken)
        {
            return await _context.RateSources.FirstOrDefaultAsync(
                       source => source.Code == DevelopmentIdentity.RateSourceCode,
                       cancellationToken)
                   ?? throw new DevelopmentRateSourceNotFoundException(
                       $"Developmentowe zrodlo kursow '{DevelopmentIdentity.RateSourceCode}' nie istnieje.");
        }

        private async Task<Currency> ResolveCurrencyAsync(
            int? currencyId,
            string? currencyCode,
            CancellationToken cancellationToken)
        {
            if (!currencyId.HasValue && string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException("Podaj currencyId albo currencyCode.");
            }

            Currency? currency = null;
            if (currencyId.HasValue)
            {
                currency = await _context.Currencies.FirstOrDefaultAsync(
                    item => item.Id == currencyId.Value,
                    cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(currencyCode))
            {
                var normalizedCode = currencyCode.Trim().ToUpperInvariant();
                var currencyByCode = await _context.Currencies.FirstOrDefaultAsync(
                    item => item.Symbol == normalizedCode,
                    cancellationToken);

                if (currency != null && currencyByCode?.Id != currency.Id)
                {
                    throw new ArgumentException("currencyId i currencyCode wskazuja rozne waluty.");
                }

                currency = currencyByCode;
            }

            return currency ?? throw new ArgumentException("Podana waluta nie istnieje.");
        }

        private async Task<bool> HasDuplicateAsync(
            int currencyId,
            int sourceId,
            DateTime effectiveDate,
            int? excludedRateId,
            CancellationToken cancellationToken)
        {
            return await _context.ExchangeRates.AnyAsync(
                rate => rate.CurrencyId == currencyId &&
                        rate.RateSourceId == sourceId &&
                        rate.EffectiveDate == effectiveDate &&
                        (!excludedRateId.HasValue || rate.Id != excludedRateId.Value),
                cancellationToken);
        }

        private async Task SaveChangesAsync(
            string currencyCode,
            DateTime effectiveDate,
            CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                throw new TestExchangeRateConflictException(
                    $"Testowy kurs {currencyCode} dla daty {effectiveDate:yyyy-MM-dd} juz istnieje.",
                    exception);
            }
        }

        private static void EnsureDevelopmentRate(ExchangeRate rate, RateSource source)
        {
            if (rate.RateSourceId != source.Id ||
                !string.Equals(
                    rate.RateSource.Code,
                    DevelopmentIdentity.RateSourceCode,
                    StringComparison.Ordinal))
            {
                throw new ProtectedExchangeRateException(
                    "Nie wolno edytowac ani usuwac kursow pochodzacych z prawdziwych zrodel.");
            }
        }

        private static void ValidateDateRange(DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value.Date > dateTo.Value.Date)
            {
                throw new ArgumentException("Data od nie moze byc pozniejsza niz data do.");
            }
        }

        private static void ValidatePrices(
            DateTime effectiveDate,
            decimal buyPrice,
            decimal sellPrice,
            decimal? midPrice)
        {
            if (effectiveDate == default)
            {
                throw new ArgumentException("Data kursu jest wymagana.");
            }

            if (buyPrice <= 0 || sellPrice <= 0 || (midPrice.HasValue && midPrice.Value <= 0))
            {
                throw new ArgumentException("Wszystkie podane ceny musza byc dodatnie.");
            }

            if (sellPrice < buyPrice)
            {
                throw new ArgumentException("Cena sprzedazy nie moze byc nizsza od ceny kupna.");
            }
        }

        private static decimal ResolveMidPrice(
            decimal buyPrice,
            decimal sellPrice,
            decimal? midPrice)
        {
            return decimal.Round(
                midPrice ?? ((buyPrice + sellPrice) / 2m),
                4,
                MidpointRounding.AwayFromZero);
        }

        private static AdminTestExchangeRateDto ToDto(ExchangeRate rate)
        {
            return new AdminTestExchangeRateDto
            {
                Id = rate.Id,
                CurrencyId = rate.CurrencyId,
                CurrencyCode = rate.Currency.Symbol,
                CurrencyName = rate.Currency.Name,
                EffectiveDate = rate.EffectiveDate,
                BuyPrice = rate.BuyPrice,
                SellPrice = rate.SellPrice,
                MidPrice = rate.MidPrice ?? ((rate.BuyPrice + rate.SellPrice) / 2m),
                RateSourceCode = rate.RateSource.Code,
                RateSourceName = rate.RateSource.Name,
                FetchedAt = rate.FetchedAt
            };
        }
    }
}
