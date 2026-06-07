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
}

export interface TradeResponse {
  success: boolean;
  message: string;
  amountTo?: number;
  fromCurrency?: string;
  toCurrency?: string;
  fromRateToPln?: number;
  toRateToPln?: number;
  sellRateSource?: string;
  buyRateSource?: string;
  effectiveDate?: string;
  newBalance?: WalletBalance;
}

export interface DepositRequest {
  currencyId: number;
  amount: number;
}

export interface WithdrawRequest {
  currencyId: number;
  amount: number;
}
