export type OrderSide = 'Buy' | 'Sell';
export type OrderStatus = 'Open' | 'PartiallyFilled' | 'Filled' | 'Cancelled';

export interface TradingPair {
  id: number;
  pair: string;
  baseCurrency: string;
  quoteCurrency: string;
  tickSize: number;
  isActive: boolean;
}

export interface OrderBookLevel {
  price: number;
  amount: number;
  total: number;
  ordersCount: number;
}

export interface OrderBook {
  pair: string;
  baseCurrency: string;
  quoteCurrency: string;
  buyOrders: OrderBookLevel[];
  sellOrders: OrderBookLevel[];
}

export interface UserOrder {
  id: number;
  pair: string;
  baseCurrency: string;
  quoteCurrency: string;
  side: OrderSide;
  type: 'Limit';
  status: OrderStatus;
  price: number;
  originalAmount: number;
  remainingAmount: number;
  createdAt: string;
  closedAt?: string | null;
}

export interface CreateOrderRequest {
  baseCurrencyCode: string;
  quoteCurrencyCode: string;
  side: OrderSide;
  price: number;
  amount: number;
}
