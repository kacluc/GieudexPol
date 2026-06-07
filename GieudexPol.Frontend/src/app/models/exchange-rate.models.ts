export interface ExchangeRateChartPoint {
  date: string;
  buyPrice: number;
  sellPrice: number;
  midPrice: number | null;
}

export interface ExchangeRateChartResponse {
  currencyCode: string;
  sourceCode: string;
  from: string;
  to: string;
  points: ExchangeRateChartPoint[];
}

export interface ExchangeRateTableRow {
  currencyCode: string;
  currencyName: string;
  sourceCode: string;
  sourceName: string;
  effectiveDate: string;
  buyPrice: number;
  sellPrice: number;
  midPrice: number | null;
}

export interface NbpSyncResult {
  from: string;
  to: string;
  added: number;
  skipped: number;
  tablesFetched: number;
  processedRanges: string[];
  warnings: string[];
}
