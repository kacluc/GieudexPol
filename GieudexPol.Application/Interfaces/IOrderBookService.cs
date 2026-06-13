using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IOrderBookService
    {
        Task<OrderDto> PlaceOrderAsync(
            int userId,
            CreateOrderRequestDto request,
            CancellationToken cancellationToken = default);

        Task<OrderDto> PlaceRateSourceOrderAsync(
            string rateSourceCode,
            CreateOrderRequestDto request,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task CancelOrderAsync(
            int userId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<OrderBookDto> GetOrderBookAsync(
            string baseCurrencyCode,
            string quoteCurrencyCode,
            int depth,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TradingPairDto>> GetTradingPairsAsync(
            CancellationToken cancellationToken = default);
    }
}
