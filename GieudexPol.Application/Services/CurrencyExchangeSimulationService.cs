using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Services;

public class CurrencyExchangeSimulationService
    : ICurrencyExchangeSimulationService
{
    private readonly IExchangeRateService _exchangeRateService;

    public CurrencyExchangeSimulationService(
        IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    public async Task<CurrencyExchangeSimulationResponseDto>
        SimulateExchangeAsync(
        CurrencyExchangeSimulationRequestDto request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException(
                "Kwota musi być większa od zera.");
        }
        ExchangeRate? exchangeRate = null;

        decimal exchangedAmount;

        if (request.SourceCurrency == request.TargetCurrency)
        {
            exchangedAmount = request.Amount;
        }
        else if (request.SourceCurrency == "PLN")
        {
            // PLN -> X

            exchangeRate =
                await _exchangeRateService.GetByCurrencyPairAsync(
                    request.TargetCurrency,
                    "PLN");

            if (exchangeRate == null)
            {
                throw new Exception(
                    "Nie znaleziono kursu walut.");
            }

            exchangedAmount =
                request.Amount / exchangeRate.SellPrice;
        }
        else if (request.TargetCurrency == "PLN")
        {
            // X -> PLN

            exchangeRate =
                await _exchangeRateService.GetByCurrencyPairAsync(
                    request.SourceCurrency,
                    "PLN");

            if (exchangeRate == null)
            {
                throw new Exception(
                    "Nie znaleziono kursu walut.");
            }

            exchangedAmount =
                request.Amount * exchangeRate.SellPrice;
        }
        else
        {
            // X -> Y

            var sourceRate =
                await _exchangeRateService.GetByCurrencyPairAsync(
                    request.SourceCurrency,
                    "PLN");

            var targetRate =
                await _exchangeRateService.GetByCurrencyPairAsync(
                    request.TargetCurrency,
                    "PLN");

            if (sourceRate == null || targetRate == null)
            {
                throw new Exception(
                    "Nie znaleziono kursu walut.");
            }

            decimal amountInPln =
                request.Amount * sourceRate.SellPrice;

            exchangedAmount =
                amountInPln / targetRate.SellPrice;

            exchangeRate = sourceRate;
        }

        decimal feeAmount =
            exchangedAmount * (request.FeePercent / 100);

        decimal finalAmount =
            exchangedAmount + feeAmount;



        var simulation = new CurrencyExchangeSimulation
        {
            Amount = request.Amount,
            SourceCurrency = request.SourceCurrency,
            TargetCurrency = request.TargetCurrency,
            ExchangeRate = exchangeRate?.SellPrice ?? 1,
            FeePercent = request.FeePercent,
            FeeAmount = feeAmount,
            FinalAmount = finalAmount,
            SimulationDate = DateTime.UtcNow
        };

        return new CurrencyExchangeSimulationResponseDto
        {
            OriginalAmount = request.Amount,
            ExchangedAmount = exchangedAmount,

            SourceCurrency = request.SourceCurrency,
            TargetCurrency = request.TargetCurrency,

            ExchangeRate = simulation.ExchangeRate,
            FeeAmount = simulation.FeeAmount,
            FinalAmount = simulation.FinalAmount,
            SimulationDate = simulation.SimulationDate
        };
    }
}
