using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Application.Services
{
    public class TransactionFeeCalculator : ITransactionFeeCalculator
    {
        private const decimal FeePercentage = 0.5m;
        private const decimal MinimumFeePln = 10m;

        private readonly ICurrencyService _currencyService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ITransactionFeeRepository _transactionFeeRepository;

        public TransactionFeeCalculator(
            ICurrencyService currencyService,
            IExchangeRateService exchangeRateService,
            ITransactionFeeRepository transactionFeeRepository)
        {
            _currencyService = currencyService;
            _exchangeRateService = exchangeRateService;
            _transactionFeeRepository = transactionFeeRepository;
        }

        public async Task<OperationFeeCalculationDto> CalculateAsync(
            string operationType,
            int currencyId,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota operacji musi byc wieksza od zera.", nameof(amount));
            }

            var currency = await _currencyService.GetByIdAsync(currencyId)
                ?? throw new InvalidOperationException("Waluta operacji nie istnieje.");
            var minimumFee = MinimumFeePln;

            if (!string.Equals(currency.Symbol, "PLN", StringComparison.OrdinalIgnoreCase))
            {
                var rate = await _exchangeRateService.GetByCurrencyPairAsync(currency.Symbol, "PLN")
                    ?? throw new InvalidOperationException(
                        $"Nie znaleziono kursu {currency.Symbol}/PLN potrzebnego do obliczenia prowizji.");
                var rateToPln = rate.MidPrice ?? ((rate.BuyPrice + rate.SellPrice) / 2m);

                if (rateToPln <= 0)
                {
                    throw new InvalidOperationException(
                        $"Kurs {currency.Symbol}/PLN potrzebny do obliczenia prowizji jest nieprawidlowy.");
                }

                minimumFee = MinimumFeePln / rateToPln;
            }

            var percentageFee = amount * FeePercentage / 100m;
            var feeAmount = decimal.Round(
                Math.Max(percentageFee, minimumFee),
                4,
                MidpointRounding.AwayFromZero);
            var feeDefinition =
                await _transactionFeeRepository.GetActiveTransactionFeeByTypeAsync(operationType);

            return new OperationFeeCalculationDto(feeAmount, feeDefinition?.Id);
        }
    }
}
