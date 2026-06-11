namespace GieudexPol.Application.DTOs
{
    public sealed record OperationFeeCalculationDto(
        decimal FeeAmount,
        Guid? TransactionFeeId);
}
