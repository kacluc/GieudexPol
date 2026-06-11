export enum AlertType {
  PriceDrop = 'PriceDrop',
  PriceIncrease = 'PriceIncrease',
  Threshold = 'Threshold',
}

export interface UserAlertCreateDto {
  currencyId: number;
  alertType: AlertType;
  thresholdValue?: number;
  percentageChange?: number;
  timeFrameHours?: number;
}

export interface UserAlertUpdateDto {
  id: number;
  currencyId: number;
  alertType: AlertType;
  thresholdValue?: number;
  percentageChange?: number;
  timeFrameHours?: number;
  isActive: boolean;
}

export interface UserAlertDto {
  id: number;
  userId: number;
  currencySymbol: string;
  alertType: AlertType;
  thresholdValue?: number;
  percentageChange?: number;
  timeFrameHours?: number;
  isActive: boolean;
  createdDate: Date;
  triggeredDate?: Date;
}
