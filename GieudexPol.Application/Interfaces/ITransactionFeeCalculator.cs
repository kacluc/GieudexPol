using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionFeeCalculator
    {
        Task<OperationFeeCalculationDto> CalculateAsync(
            string operationType,
            int currencyId,
            decimal amount,
            CancellationToken cancellationToken = default);
    }
}
