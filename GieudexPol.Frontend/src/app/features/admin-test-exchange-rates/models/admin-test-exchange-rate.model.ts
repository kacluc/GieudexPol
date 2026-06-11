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

export interface AdminTestRateSource {
  code: string;
  name: string;
}

export interface AdminTestExchangeRateFilters {
  rateSourceCode?: string;
  currencyId?: number;
  currencyCode?: string;
  dateFrom?: string;
  dateTo?: string;
}

export interface CreateTestExchangeRate {
  rateSourceCode: string;
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
