using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces;

public interface ICurrencyExchangeSimulationService
{
    Task<CurrencyExchangeSimulationResponseDto> SimulateExchangeAsync(
        CurrencyExchangeSimulationRequestDto request);
}