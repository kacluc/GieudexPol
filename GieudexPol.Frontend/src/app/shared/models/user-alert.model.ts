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
  isActive: boolean;
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
  isActive: boolean;
  createdDate: string;
  triggeredDate?: string | null;
  isAcknowledged: boolean;
  acknowledgedDate?: string | null;
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
  isActive: boolean;
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
  isActive: boolean;
  createdDate: string;
  triggeredDate?: string | null;
  isAcknowledged: boolean;
  acknowledgedDate?: string | null;
}
