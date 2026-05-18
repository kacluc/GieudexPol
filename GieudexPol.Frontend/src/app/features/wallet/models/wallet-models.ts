export interface WalletBalance {
  currencyCode: string;
  balance: number;
}

export interface WalletCurrency {
  id: number;
  symbol: string;
  name: string;
  isActive: boolean;
}

export interface WalletDto {
  id: number;
  userId: number;
  currencyId: number;
  currency?: WalletCurrency;
  balance: number;
}

export interface TradeRequest {
  fromCurrencyId: number;
  amountFrom: number;
  toCurrencyId: number;
  amountTo: number;
}

export interface TradeResponse {
  success: boolean;
  message: string;
  newBalance?: WalletBalance;
}
