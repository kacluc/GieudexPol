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
