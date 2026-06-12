using System;
using System.Security.Claims;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExchangeRatesController : ControllerBase
    {
        private static readonly DateTime MinimumSyncDate = new DateTime(2026, 1, 1);
        private static readonly HashSet<string> SyncableSources = new(StringComparer.OrdinalIgnoreCase)
        {
            "NBP",
            "ECB",
            "RIKSBANK",
            "BOE",
            "BOC",
            "CNB",
            "NORGES",
            "BNR"
        };
        private static readonly HashSet<string> SyntheticRateSources =
            new(SyncableSources.Where(code => code != "NBP"), StringComparer.OrdinalIgnoreCase);

        private readonly IExchangeRateService _exchangeRateService;
        private readonly IExchangeRateSyncService _exchangeRateSyncService;

        public ExchangeRatesController(
            IExchangeRateService exchangeRateService,
            IExchangeRateSyncService exchangeRateSyncService)
        {
            _exchangeRateService = exchangeRateService;
            _exchangeRateSyncService = exchangeRateSyncService;
        }

        [HttpGet("{baseCurrencySymbol}/{targetCurrencySymbol}")]
        public async Task<IActionResult> GetExchangeRateByCurrencyPair(string baseCurrencySymbol, string targetCurrencySymbol)
        {
            var exchangeRate = await _exchangeRateService.GetByCurrencyPairAsync(baseCurrencySymbol, targetCurrencySymbol);
            if (exchangeRate == null)
            {
                return NotFound();
            }

            if (!CanAccessSource(exchangeRate.RateSource.Code))
            {
                return NotFound();
            }

            return Ok(exchangeRate);
        }

        [HttpGet("chart")]
        public async Task<IActionResult> GetChartData(
            [FromQuery] string currency,
            [FromQuery] string source,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                return BadRequest("Currency query parameter is required.");
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                return BadRequest("Source query parameter is required.");
            }

            var (resolvedFrom, resolvedTo) = ResolveDateRange(from, to);

            if (resolvedFrom > resolvedTo)
            {
                return BadRequest("From date cannot be later than to date.");
            }

            if (resolvedFrom < MinimumSyncDate)
            {
                return BadRequest("From date cannot be earlier than 2026-01-01.");
            }

            if (resolvedTo > DateTime.Today)
            {
                return BadRequest("To date cannot be later than today.");
            }

            var currencyCode = currency.Trim().ToUpperInvariant();
            var sourceCode = source.Trim().ToUpperInvariant();
            if (!CanAccessSource(sourceCode))
            {
                return Forbid();
            }

            var chartData = await _exchangeRateService.GetChartDataAsync(
                currencyCode,
                sourceCode,
                resolvedFrom,
                resolvedTo);

            var expectedPublicationDate = ResolveExpectedPublicationDate(resolvedTo);
            var requiresSyntheticRefresh =
                SyntheticRateSources.Contains(sourceCode) &&
                chartData.Points.Any(point => point.BuyPrice == point.SellPrice);
            var shouldSynchronize = IsSyncableSource(sourceCode) &&
                (requiresSyntheticRefresh ||
                 chartData.Points.Count == 0 ||
                 chartData.Points.All(point => point.Date.Date != expectedPublicationDate));

            if (shouldSynchronize)
            {
                try
                {
                    var syncFrom = requiresSyntheticRefresh || chartData.Points.Count == 0
                        ? resolvedFrom
                        : expectedPublicationDate;

                    await _exchangeRateSyncService.SyncRatesAsync(
                        sourceCode,
                        syncFrom,
                        resolvedTo,
                        cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }

                chartData = await _exchangeRateService.GetChartDataAsync(
                    currencyCode,
                    sourceCode,
                    resolvedFrom,
                    resolvedTo);
            }

            return Ok(chartData);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRates(
            [FromQuery] string source = "NBP",
            [FromQuery] string? currency = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = "NBP";
            }

            var sourceCode = source.Trim().ToUpperInvariant();
            if (!CanAccessSource(sourceCode))
            {
                return Forbid();
            }

            var currencyCode = string.IsNullOrWhiteSpace(currency)
                ? null
                : currency.Trim().ToUpperInvariant();

            var rates = (await _exchangeRateService.GetLatestRatesAsync(sourceCode, currencyCode)).ToList();
            var currentYearStart = new DateTime(DateTime.Today.Year, 1, 1);
            var requiresSyntheticRefresh =
                SyntheticRateSources.Contains(sourceCode) &&
                rates.Any(rate => rate.BuyPrice == rate.SellPrice);

            if ((requiresSyntheticRefresh ||
                 !rates.Any() ||
                 rates.All(rate => rate.EffectiveDate < currentYearStart)) &&
                IsSyncableSource(sourceCode))
            {
                try
                {
                    await _exchangeRateSyncService.SyncCurrentYearRatesAsync(sourceCode, cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }

                rates = (await _exchangeRateService.GetLatestRatesAsync(sourceCode, currencyCode)).ToList();
            }

            return Ok(rates);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExchangeRates()
        {
            var exchangeRates = await _exchangeRateService.GetAllAsync();
            if (!IsDevelopmentUser() && !User.IsInRole(UserRoles.Admin))
            {
                exchangeRates = exchangeRates.Where(rate =>
                    !string.Equals(
                        rate.RateSource.Code,
                        DevelopmentIdentity.RateSourceCode,
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        rate.RateSource.Code,
                        DevelopmentIdentity.RateSourceCodeB,
                        StringComparison.OrdinalIgnoreCase));
            }

            return Ok(exchangeRates);
        }

        [HttpPost("sync/nbp")]
        public async Task<IActionResult> SyncNbpRates(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            CancellationToken cancellationToken)
        {
            if (from.Date > to.Date)
            {
                return BadRequest("From date cannot be later than to date.");
            }

            if (from.Date < MinimumSyncDate)
            {
                return BadRequest("NBP sync from date cannot be earlier than 2026-01-01.");
            }

            if (to.Date > DateTime.Today)
            {
                return BadRequest("NBP sync to date cannot be later than today.");
            }

            var result = await _exchangeRateSyncService.SyncNbpRatesAsync(
                from.Date,
                to.Date,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("sync/ecb")]
        public async Task<IActionResult> SyncEcbRates(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            return await SyncRatesBySource("ECB", from, to, cancellationToken);
        }

        [HttpPost("sync/riksbank")]
        public async Task<IActionResult> SyncRiksbankRates(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            return await SyncRatesBySource("RIKSBANK", from, to, cancellationToken);
        }

        [HttpPost("sync/boe")]
        public async Task<IActionResult> SyncBankOfEnglandRates(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            return await SyncRatesBySource("BOE", from, to, cancellationToken);
        }

        [HttpPost("sync/boc")]
        public async Task<IActionResult> SyncBankOfCanadaRates(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            return await SyncRatesBySource("BOC", from, to, cancellationToken);
        }

        [HttpPost("sync/cnb")]
        public async Task<IActionResult> SyncCzechNationalBankRates(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            return await SyncRatesBySource("CNB", from, to, cancellationToken);
        }

        [HttpPost("sync/norges")]
        public async Task<IActionResult> SyncNorgesBankRates(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            return await SyncRatesBySource("NORGES", from, to, cancellationToken);
        }

        [HttpPost("sync/bnr")]
        public async Task<IActionResult> SyncNationalBankOfRomaniaRates(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            return await SyncRatesBySource("BNR", from, to, cancellationToken);
        }

        [HttpPost("sync/{sourceCode}")]
        public async Task<IActionResult> SyncRatesBySource(
            string sourceCode,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                return BadRequest("Source code is required.");
            }

            sourceCode = sourceCode.Trim().ToUpperInvariant();
            if (!IsSyncableSource(sourceCode))
            {
                return BadRequest($"Exchange rate source '{sourceCode}' is not supported for synchronization.");
            }

            var (resolvedFrom, resolvedTo) = ResolveDateRange(from, to);

            if (resolvedFrom > resolvedTo)
            {
                return BadRequest("From date cannot be later than to date.");
            }

            if (resolvedFrom < MinimumSyncDate)
            {
                return BadRequest("From date cannot be earlier than 2026-01-01.");
            }

            if (resolvedTo > DateTime.Today)
            {
                return BadRequest("To date cannot be later than today.");
            }

            try
            {
                var result = await _exchangeRateSyncService.SyncRatesAsync(
                    sourceCode,
                    resolvedFrom,
                    resolvedTo,
                    cancellationToken);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateExchangeRate([FromBody] ExchangeRate exchangeRate)
        {
            await _exchangeRateService.AddAsync(exchangeRate);
            return CreatedAtAction(nameof(GetExchangeRateByCurrencyPair), new { id = exchangeRate.Id }, exchangeRate);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExchangeRate(int id, [FromBody] ExchangeRate exchangeRate)
        {
            if (id != exchangeRate.Id)
            {
                return BadRequest();
            }
            await _exchangeRateService.UpdateAsync(exchangeRate);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExchangeRate(int id)
        {
            var exchangeRate = await _exchangeRateService.GetByIdAsync(id);
            if (exchangeRate == null)
            {
                return NotFound();
            }
            await _exchangeRateService.DeleteAsync(exchangeRate);
            return NoContent();
        }

        private static (DateTime From, DateTime To) ResolveDateRange(DateTime? from, DateTime? to)
        {
            var today = DateTime.Today;
            return (
                from?.Date ?? new DateTime(today.Year, 1, 1),
                to?.Date ?? today);
        }

        private static bool IsSyncableSource(string sourceCode)
        {
            return SyncableSources.Contains(sourceCode);
        }

        private bool CanAccessSource(string sourceCode)
        {
            var isDevelopmentSource =
                string.Equals(
                    sourceCode,
                    DevelopmentIdentity.RateSourceCode,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    sourceCode,
                    DevelopmentIdentity.RateSourceCodeB,
                    StringComparison.OrdinalIgnoreCase);

            return !isDevelopmentSource ||
                   User.IsInRole(UserRoles.Admin) ||
                   IsDevelopmentUser();
        }

        private bool IsDevelopmentUser()
        {
            return string.Equals(
                User.FindFirstValue(ClaimTypes.Email),
                DevelopmentIdentity.UserEmail,
                StringComparison.OrdinalIgnoreCase);
        }

        private static DateTime ResolveExpectedPublicationDate(DateTime date)
        {
            var expectedDate = date.Date;

            while (expectedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                expectedDate = expectedDate.AddDays(-1);
            }

            return expectedDate;
        }
    }
}
