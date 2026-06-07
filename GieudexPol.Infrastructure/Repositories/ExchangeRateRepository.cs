using GieudexPol.Application.Interfaces;
using GieudexPol.Application.DTOs;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class ExchangeRateRepository : GenericRepository<ExchangeRate>, IExchangeRateRepository
    {
        public ExchangeRateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ExchangeRate?> GetByCurrencyPairAsync(string baseCurrencySymbol, string targetCurrencySymbol)
        {
            if (targetCurrencySymbol != "PLN")
            {
                return null;
            }

            return await _dbSet
                .Include(er => er.Currency)
                .Include(er => er.RateSource)
                .Where(er => er.Currency.Symbol == baseCurrencySymbol)
                .OrderByDescending(er => er.EffectiveDate)
                .ThenByDescending(er => er.FetchedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ExchangeRateChartPointDto>> GetChartDataAsync(
            string currencyCode,
            string sourceCode,
            DateTime from,
            DateTime to)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(er =>
                    er.Currency.Symbol == currencyCode &&
                    er.RateSource.Code == sourceCode &&
                    er.EffectiveDate >= from.Date &&
                    er.EffectiveDate <= to.Date)
                .OrderBy(er => er.EffectiveDate)
                .Select(er => new ExchangeRateChartPointDto
                {
                    Date = er.EffectiveDate,
                    BuyPrice = er.BuyPrice,
                    SellPrice = er.SellPrice,
                    MidPrice = er.MidPrice
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ExchangeRateChartPointDto>> GetRatesForChartAsync(
            string currencySymbol,
            string sourceCode,
            DateTime from,
            DateTime to)
        {
            return await GetChartDataAsync(currencySymbol, sourceCode, from, to);
        }

        public async Task<IEnumerable<ExchangeRateTableRowDto>> GetLatestRatesAsync(string sourceCode, string? currencyCode = null)
        {
            var ratesForSource = _dbSet
                .AsNoTracking()
                .Where(er => er.RateSource.Code == sourceCode);

            if (!string.IsNullOrWhiteSpace(currencyCode))
            {
                ratesForSource = ratesForSource.Where(er => er.Currency.Symbol == currencyCode);
            }

            var latestRatesQuery = ratesForSource
                .GroupBy(er => er.CurrencyId)
                .Select(group => new
                {
                    CurrencyId = group.Key,
                    EffectiveDate = group.Max(er => er.EffectiveDate)
                });

            return await ratesForSource
                .Join(
                    latestRatesQuery,
                    rate => new { rate.CurrencyId, rate.EffectiveDate },
                    latest => new { latest.CurrencyId, latest.EffectiveDate },
                    (rate, latest) => rate)
                .OrderBy(er => er.Currency.Symbol)
                .Select(er => new ExchangeRateTableRowDto
                {
                    CurrencyCode = er.Currency.Symbol,
                    CurrencyName = er.Currency.Name,
                    SourceCode = er.RateSource.Code,
                    SourceName = er.RateSource.Name,
                    EffectiveDate = er.EffectiveDate,
                    BuyPrice = er.BuyPrice,
                    SellPrice = er.SellPrice,
                    MidPrice = er.MidPrice
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ExchangeRate>> GetTradingRateCandidatesAsync(
            IReadOnlyCollection<int> currencyIds,
            DateTime oldestAcceptedDate,
            DateTime notAfter)
        {
            var requestedCurrencyIds = currencyIds.Distinct().ToList();
            if (requestedCurrencyIds.Count == 0)
            {
                return Array.Empty<ExchangeRate>();
            }

            var applicableDate = await _dbSet
                .AsNoTracking()
                .Where(rate =>
                    requestedCurrencyIds.Contains(rate.CurrencyId) &&
                    rate.EffectiveDate >= oldestAcceptedDate.Date &&
                    rate.EffectiveDate <= notAfter.Date)
                .GroupBy(rate => rate.EffectiveDate)
                .Where(group => group.Select(rate => rate.CurrencyId).Distinct().Count() == requestedCurrencyIds.Count)
                .Select(group => (DateTime?)group.Key)
                .OrderByDescending(date => date)
                .FirstOrDefaultAsync();

            if (!applicableDate.HasValue)
            {
                return Array.Empty<ExchangeRate>();
            }

            return await _dbSet
                .AsNoTracking()
                .Include(rate => rate.Currency)
                .Include(rate => rate.RateSource)
                .Where(rate =>
                    requestedCurrencyIds.Contains(rate.CurrencyId) &&
                    rate.EffectiveDate == applicableDate.Value)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int currencyId, int rateSourceId, DateTime effectiveDate)
        {
            return await _dbSet.AnyAsync(er =>
                er.CurrencyId == currencyId &&
                er.RateSourceId == rateSourceId &&
                er.EffectiveDate == effectiveDate);
        }
    }
}
