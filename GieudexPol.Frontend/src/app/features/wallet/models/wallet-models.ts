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
  reservedBalance: number;
  availableBalance: number;
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
  rateSource?: string;
  appliedRate?: number;
  feeAmount?: number;
  feeCurrency?: string;
  exchangeExecutionId?: number;
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

export interface ExchangePreviewRequest {
  fromCurrencyId: number;
  toCurrencyId: number;
  amount: number;
}

export interface ExchangePreviewResult {
  fromCurrencyCode: string;
  toCurrencyCode: string;
  inputAmount: number;
  estimatedOutputAmount: number;
  rate: number;
  feeAmount: number;
  feeCurrencyCode: string;
  totalDebitAmount: number;
  rateDate: string;
  rateSourceCode?: string;
  rateSourceName?: string;
  hasSufficientFunds: boolean;
  isPreview: boolean;
  message: string;
}
