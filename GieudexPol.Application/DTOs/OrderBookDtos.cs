using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.DTOs
{
    public class CreateOrderRequestDto
    {
        public string BaseCurrencyCode { get; set; } = string.Empty;
        public string QuoteCurrencyCode { get; set; } = string.Empty;
        public OrderSide Side { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreateRateSourceOrderRequestDto : CreateOrderRequestDto
    {
        public string RateSourceCode { get; set; } = string.Empty;
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string Pair { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = string.Empty;
        public string QuoteCurrency { get; set; } = string.Empty;
        public OrderSide Side { get; set; }
        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    public class OrderBookLevelDto
    {
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public int OrdersCount { get; set; }
    }

    public class OrderBookDto
    {
        public string Pair { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = string.Empty;
        public string QuoteCurrency { get; set; } = string.Empty;
        public IReadOnlyList<OrderBookLevelDto> BuyOrders { get; set; } = [];
        public IReadOnlyList<OrderBookLevelDto> SellOrders { get; set; } = [];
    }

    public class TradingPairDto
    {
        public int Id { get; set; }
        public string Pair { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = string.Empty;
        public string QuoteCurrency { get; set; } = string.Empty;
        public decimal TickSize { get; set; }
        public bool IsActive { get; set; }
    }
}
