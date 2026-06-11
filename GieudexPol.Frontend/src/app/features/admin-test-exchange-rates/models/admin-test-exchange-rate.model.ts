export interface AdminTestExchangeRate {
  id: number;
  currencyId: number;
  currencyCode: string;
  currencyName: string;
  effectiveDate: string;
  buyPrice: number;
  sellPrice: number;
  midPrice: number;
  rateSourceCode: string;
  rateSourceName: string;
  fetchedAt: string;
}

export interface AdminTestExchangeRateFilters {
  currencyId?: number;
  currencyCode?: string;
  dateFrom?: string;
  dateTo?: string;
}

export interface CreateTestExchangeRate {
  currencyId?: number;
  currencyCode?: string;
  effectiveDate: string;
  buyPrice: number;
  sellPrice: number;
  midPrice?: number;
}

export interface UpdateTestExchangeRate {
  effectiveDate: string;
  buyPrice: number;
  sellPrice: number;
  midPrice?: number;
}
