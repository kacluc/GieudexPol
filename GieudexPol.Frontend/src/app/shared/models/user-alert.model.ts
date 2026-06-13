export enum AlertType {
  PriceDrop = 'PriceDrop',
  PriceIncrease = 'PriceIncrease',
  Threshold = 'Threshold',
}

export enum AlertPriceSide {
  UserBuysCurrency = 'UserBuysCurrency',
  UserSellsCurrency = 'UserSellsCurrency',
  MidPrice = 'MidPrice',
}

export enum ThresholdDirection {
  AboveOrEqual = 'AboveOrEqual',
  BelowOrEqual = 'BelowOrEqual',
}

export enum AlertStatus {
  Active = 'Active',
  Fulfilled = 'Fulfilled',
  Inactive = 'Inactive',
}

export interface AlertLogDto {
  id: number;
  message: string;
  createdDate: string;
  currentPrice?: number | null;
  currentAmount?: number | null;
  sourceSummary?: string | null;
  effectiveDate?: string | null;
}

export interface UserAlertCreateDto {
  currencyId: number;
  alertType: AlertType;
  priceSide: AlertPriceSide;
  thresholdDirection?: ThresholdDirection | null;
  rateSourceId?: number | null;
  thresholdValue?: number | null;
  percentageChange?: number | null;
  timeFrameHours?: number | null;
}

export interface UserAlertUpdateDto {
  id: number;
  currencyId: number;
  alertType: AlertType;
  priceSide: AlertPriceSide;
  thresholdDirection?: ThresholdDirection | null;
  rateSourceId?: number | null;
  thresholdValue?: number | null;
  percentageChange?: number | null;
  timeFrameHours?: number | null;
  status: AlertStatus;
}

export interface UserAlertDto {
  id: number;
  userId: number;
  currencyId: number;
  currencySymbol: string;
  alertType: AlertType;
  priceSide: AlertPriceSide;
  thresholdDirection?: ThresholdDirection | null;
  rateSourceId?: number | null;
  rateSourceCode?: string | null;
  rateSourceName?: string | null;
  appliesToAllRateSources: boolean;
  thresholdValue?: number | null;
  percentageChange?: number | null;
  timeFrameHours?: number | null;
  status: AlertStatus;
  createdDate: string;
  triggeredDate?: string | null;
  logs: AlertLogDto[];
}

export interface AlertRateSource {
  id: number;
  code: string;
  name: string;
}

export enum TradingAlertEvent {
  BuyOrder = 'BuyOrder',
  SellOrder = 'SellOrder',
  TradeExecution = 'TradeExecution',
}

export interface TradingPairOption {
  id: number;
  pair: string;
  baseCurrency: string;
  quoteCurrency: string;
  tickSize: number;
  isActive: boolean;
}

export interface UserTradingAlertCreateDto {
  tradingPairId: number;
  eventType: TradingAlertEvent;
  direction: ThresholdDirection;
  targetPrice: number;
  minimumAmount?: number | null;
}

export interface UserTradingAlertUpdateDto extends UserTradingAlertCreateDto {
  id: number;
  status: AlertStatus;
}

export interface UserTradingAlertDto {
  id: number;
  tradingPairId: number;
  pair: string;
  baseCurrency: string;
  quoteCurrency: string;
  eventType: TradingAlertEvent;
  direction: ThresholdDirection;
  targetPrice: number;
  minimumAmount?: number | null;
  status: AlertStatus;
  createdDate: string;
  triggeredDate?: string | null;
  logs: AlertLogDto[];
}
