import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { timeout } from 'rxjs/operators';
import { AuthService } from '../features/auth/services/auth.service';
import {
  ExchangeRateChartPoint,
  ExchangeRateTableRow,
} from '../models/exchange-rate.models';
import { ExchangeRateApiService } from '../services/exchange-rate-api.service';

@Component({
  selector: 'app-exchange-rate-dashboard',
  imports: [CommonModule, FormsModule],
  templateUrl: './exchange-rate-dashboard.html',
  styleUrl: './exchange-rate-dashboard.css',
})
export class ExchangeRateDashboard implements OnInit {
  private readonly developmentUserEmail = 'dev@gieudexpol.local';
  private readonly nbpSourceCode = 'NBP';
  private readonly ecbSourceCode = 'ECB';
  private readonly riksbankSourceCode = 'RIKSBANK';
  private readonly boeSourceCode = 'BOE';
  private readonly bocSourceCode = 'BOC';
  private readonly cnbSourceCode = 'CNB';
  private readonly norgesSourceCode = 'NORGES';
  private readonly bnrSourceCode = 'BNR';
  private readonly mockSourceCode = 'MOCK_BANK_A';
  private readonly mockSourceCodeB = 'MOCK_BANK_B';
  private readonly nbpBuySellCurrencyCodes = new Set([
    'AUD',
    'CAD',
    'CHF',
    'CZK',
    'DKK',
    'EUR',
    'GBP',
    'HUF',
    'JPY',
    'NOK',
    'SEK',
    'USD',
  ]);
  private readonly ecbCurrencyCodes = new Set([
    'AUD',
    'CAD',
    'CHF',
    'CZK',
    'DKK',
    'EUR',
    'GBP',
    'HUF',
    'JPY',
    'KRW',
    'NOK',
    'RON',
    'SEK',
    'TRY',
    'USD',
  ]);
  private readonly riksbankCurrencyCodes = new Set([
    'AUD',
    'CAD',
    'CHF',
    'CZK',
    'DKK',
    'EUR',
    'GBP',
    'HUF',
    'JPY',
    'KRW',
    'NOK',
    'RON',
    'SEK',
    'TRY',
    'USD',
  ]);
  private readonly boeCurrencyCodes = new Set([
    'AUD',
    'CAD',
    'CHF',
    'CZK',
    'DKK',
    'EUR',
    'GBP',
    'HUF',
    'JPY',
    'KRW',
    'NOK',
    'RON',
    'SEK',
    'TRY',
    'USD',
  ]);
  private readonly bocCurrencyCodes = new Set([
    'CAD',
    'CHF',
    'EUR',
    'GBP',
    'JPY',
    'USD',
  ]);
  private readonly cnbCurrencyCodes = new Set([
    'AUD',
    'CAD',
    'CHF',
    'CZK',
    'DKK',
    'EUR',
    'GBP',
    'HUF',
    'JPY',
    'KRW',
    'NOK',
    'RON',
    'SEK',
    'TRY',
    'USD',
  ]);
  private readonly norgesCurrencyCodes = new Set([
    'AUD',
    'CAD',
    'CHF',
    'CZK',
    'DKK',
    'EUR',
    'GBP',
    'HUF',
    'JPY',
    'KRW',
    'NOK',
    'RON',
    'SEK',
    'TRY',
    'USD',
  ]);
  private readonly bnrCurrencyCodes = new Set([
    'AUD',
    'CAD',
    'CHF',
    'CZK',
    'DKK',
    'EUR',
    'GBP',
    'HUF',
    'JPY',
    'KRW',
    'NOK',
    'RON',
    'SEK',
    'TRY',
    'USD',
  ]);
  readonly currencies = [
    { code: 'EUR', label: 'Euro' },
    { code: 'USD', label: 'Dolar amerykanski' },
    { code: 'CHF', label: 'Frank szwajcarski' },
    { code: 'GBP', label: 'Funt brytyjski' },
    { code: 'HUF', label: 'Forint wegierski' },
    { code: 'CZK', label: 'Korona czeska' },
    { code: 'DKK', label: 'Korona dunska' },
    { code: 'SEK', label: 'Korona szwedzka' },
    { code: 'NOK', label: 'Korona norweska' },
    { code: 'RON', label: 'Lej rumunski' },
    { code: 'TRY', label: 'Lira turecka' },
    { code: 'UAH', label: 'Hrywna ukrainska' },
    { code: 'AUD', label: 'Dolar australijski' },
    { code: 'CAD', label: 'Dolar kanadyjski' },
    { code: 'JPY', label: 'Jen japonski' },
    { code: 'KRW', label: 'Won poludniowokoreanski' },
  ];
  private readonly availableSources = [
    {
      code: 'MOCK_BANK_A',
      label: 'MOCK_BANK_A - dane testowe',
    },
    {
      code: 'MOCK_BANK_B',
      label: 'MOCK_BANK_B - dane testowe',
    },
    {
      code: 'NBP',
      label: 'NBP - realne kursy',
    },
    {
      code: 'ECB',
      label: 'ECB - kursy referencyjne',
    },
    {
      code: 'RIKSBANK',
      label: 'RIKSBANK - kursy referencyjne',
    },
    {
      code: 'BOE',
      label: 'BOE - publikowane kursy spot',
    },
    {
      code: 'BOC',
      label: 'BOC - kursy referencyjne',
    },
    {
      code: 'CNB',
      label: 'CNB - kursy referencyjne',
    },
    {
      code: 'NORGES',
      label: 'NORGES - kursy referencyjne',
    },
    {
      code: 'BNR',
      label: 'BNR - kursy referencyjne',
    },
  ];

  get isDevelopmentUser(): boolean {
    return localStorage.getItem('userEmail')?.toLowerCase() === this.developmentUserEmail;
  }

  get canAccessTestSources(): boolean {
    return this.isDevelopmentUser || this.authService.isAdmin();
  }

  get sources(): Array<{ code: string; label: string }> {
    return this.canAccessTestSources
      ? this.availableSources
      : this.availableSources.filter(
          source =>
            source.code !== this.mockSourceCode &&
            source.code !== this.mockSourceCodeB,
        );
  }
  readonly rangePresets = [
    { label: '7D', days: 7 },
    { label: '30D', days: 30 },
    { label: '90D', days: 90 },
    { label: 'YTD', ytd: true },
    { label: 'DEV', from: '2026-01-01' },
  ];
  readonly minimumRateDate = '2026-01-01';
  readonly maximumRateDate = this.formatDateInput(new Date());
  readonly chartWidth = 920;
  readonly chartHeight = 320;
  readonly chartPadding = {
    top: 24,
    right: 26,
    bottom: 44,
    left: 58,
  };

  currency = 'EUR';
  source = this.nbpSourceCode;
  from = '2026-01-01';
  to = this.maximumRateDate;

  chartPoints: ExchangeRateChartPoint[] = [];
  tablePoints: ExchangeRateChartPoint[] = [];
  latestRates: ExchangeRateTableRow[] = [];
  loading = false;
  syncingNbp = false;
  errorMessage = '';
  statusMessage = 'Gotowe do pobrania danych.';
  lastLoadedAt: Date | null = null;
  lastNbpSyncSummary = '';
  selectedPointIndex: number | null = null;

  constructor(
    private readonly exchangeRateApi: ExchangeRateApiService,
    private readonly changeDetector: ChangeDetectorRef,
    private readonly authService: AuthService,
  ) {}

  ngOnInit(): void {
    if (this.isDevelopmentUser) {
      this.source = this.mockSourceCode;
    }

    this.fetchData();
  }

  async fetchData(): Promise<void> {
    if (this.loading || this.syncingNbp) {
      return;
    }

    await this.fetchDataWithOptionalNbpSync();
  }

  onSourceChange(source: string): void {
    this.source = source;
    this.ensureSelectedCurrencyIsAvailable();
    void this.fetchData();
  }

  private async fetchDataWithOptionalNbpSync(): Promise<void> {
    this.errorMessage = '';
    this.statusMessage = this.source === this.nbpSourceCode
      ? 'Sprawdzam lokalne dane NBP...'
      : 'Pobieram dane z backendu...';
    this.selectedPointIndex = null;

    if (!this.validateFilters()) {
      return;
    }

    this.loading = true;

    try {
      await this.loadDataFromBackend();

      if (this.shouldAutoSyncNbp()) {
        this.loading = false;
        this.statusMessage = 'Nie ma lokalnych danych NBP dla tego zakresu. Pobieram z NBP...';
        this.changeDetector.detectChanges();

        const synced = await this.runNbpSync();

        if (synced) {
          await this.fetchDataWithoutAutoSync();
        }

        return;
      }

      this.finishFetchStatus();
    } finally {
      this.loading = false;
      this.changeDetector.detectChanges();
    }
  }

  private async fetchDataWithoutAutoSync(): Promise<void> {
    this.errorMessage = '';
    this.statusMessage = 'Pobieram zapisane dane z backendu...';
    this.selectedPointIndex = null;

    if (!this.validateFilters()) {
      return;
    }

    this.loading = true;

    try {
      await this.loadDataFromBackend();
      this.finishFetchStatus();
    } finally {
      this.loading = false;
      this.changeDetector.detectChanges();
    }
  }

  private async runNbpSync(): Promise<boolean> {
    this.errorMessage = '';
    this.lastNbpSyncSummary = '';
    this.statusMessage = 'Synchronizuje kursy z NBP...';

    if (!this.validateFilters()) {
      return false;
    }

    this.syncingNbp = true;

    try {
      const result = await firstValueFrom(
        this.exchangeRateApi
          .syncNbpRates(this.from, this.to)
          .pipe(timeout(60000)),
      );

      this.lastNbpSyncSummary =
        `NBP: dodano ${result.added}, pominieto ${result.skipped}, tabel ${result.tablesFetched}.`;
      this.statusMessage = 'Synchronizacja NBP zakonczona. Odswiezam dane...';
      return true;
    } catch {
      this.errorMessage = 'Nie udalo sie zsynchronizowac kursow z NBP. Sprawdz czy backend ma dostep do internetu i czy zakres dat jest poprawny.';
      this.statusMessage = 'Synchronizacja NBP zakonczona bledem.';
      return false;
    } finally {
      this.syncingNbp = false;
      this.changeDetector.detectChanges();
    }
  }

  get latestPoint(): ExchangeRateChartPoint | null {
    return this.chartPoints.at(-1) ?? null;
  }

  get latestSelectedRate(): ExchangeRateTableRow | null {
    return this.latestRates.find((rate) => rate.currencyCode === this.currency) ?? null;
  }

  get selectedPoint(): ExchangeRateChartPoint | null {
    if (this.selectedPointIndex === null) {
      return null;
    }

    return this.chartPoints[this.selectedPointIndex] ?? null;
  }

  spread(point: ExchangeRateChartPoint): number {
    return point.sellPrice - point.buyPrice;
  }

  get chartMinValue(): number {
    const values = this.chartValues;
    return values.length ? Math.min(...values) : 0;
  }

  get chartMaxValue(): number {
    const values = this.chartValues;
    return values.length ? Math.max(...values) : 1;
  }

  get chartRange(): number {
    const range = this.chartMaxValue - this.chartMinValue;
    return range === 0 ? 1 : range;
  }

  get gridLines(): Array<{ y: number; label: number }> {
    const lines = 4;

    return Array.from({ length: lines + 1 }, (_, index) => {
      const ratio = index / lines;
      const label = this.chartMaxValue - this.chartRange * ratio;

      return {
        y: this.chartPadding.top + this.plotHeight * ratio,
        label,
      };
    });
  }

  get buyLinePoints(): string {
    return this.createLinePoints('buyPrice');
  }

  get sellLinePoints(): string {
    return this.createLinePoints('sellPrice');
  }

  get firstPointDate(): string {
    return this.chartPoints[0]?.date ?? '';
  }

  get lastPointDate(): string {
    return this.chartPoints.at(-1)?.date ?? '';
  }

  get selectedPointX(): number {
    return this.selectedPointIndex === null ? 0 : this.getPointX(this.selectedPointIndex);
  }

  get selectedBuyY(): number {
    const point = this.selectedPoint;
    return point ? this.getPointY(point.buyPrice) : 0;
  }

  get selectedSellY(): number {
    const point = this.selectedPoint;
    return point ? this.getPointY(point.sellPrice) : 0;
  }

  get selectedTooltipX(): number {
    return Math.min(Math.max(this.selectedPointX - 70, this.chartPadding.left), this.chartWidth - 190);
  }

  get selectedTooltipY(): number {
    return Math.max(Math.min(Math.min(this.selectedBuyY, this.selectedSellY) - 88, this.chartHeight - 128), 12);
  }

  get plotWidth(): number {
    return this.chartWidth - this.chartPadding.left - this.chartPadding.right;
  }

  get plotHeight(): number {
    return this.chartHeight - this.chartPadding.top - this.chartPadding.bottom;
  }

  trackByDate(_: number, point: ExchangeRateChartPoint): string {
    return point.date;
  }

  trackByCurrency(_: number, rate: ExchangeRateTableRow): string {
    return `${rate.sourceCode}-${rate.currencyCode}`;
  }

  selectChartPoint(event: MouseEvent): void {
    if (this.chartPoints.length === 0) {
      this.selectedPointIndex = null;
      return;
    }

    const target = event.currentTarget as SVGSVGElement;
    const bounds = target.getBoundingClientRect();
    const ratio = (event.clientX - bounds.left) / bounds.width;
    const x = ratio * this.chartWidth;
    const clampedX = Math.min(
      Math.max(x, this.chartPadding.left),
      this.chartWidth - this.chartPadding.right,
    );
    const plotRatio = (clampedX - this.chartPadding.left) / this.plotWidth;
    const index = Math.round(plotRatio * (this.chartPoints.length - 1));

    this.selectedPointIndex = Math.min(Math.max(index, 0), this.chartPoints.length - 1);
  }

  clearSelectedPoint(): void {
    this.selectedPointIndex = null;
  }

  selectCurrency(currency: string): void {
    if (!this.isCurrencyAvailableForSource(currency, this.source)) {
      return;
    }

    this.currency = currency;
    void this.fetchData();
  }

  isCurrencyAvailableForSource(currencyCode: string, sourceCode: string = this.source): boolean {
    if (
      sourceCode === this.mockSourceCode ||
      sourceCode === this.mockSourceCodeB
    ) {
      return true;
    }

    if (sourceCode === this.nbpSourceCode) {
      return this.nbpBuySellCurrencyCodes.has(currencyCode);
    }

    if (sourceCode === this.ecbSourceCode) {
      return this.ecbCurrencyCodes.has(currencyCode);
    }

    if (sourceCode === this.riksbankSourceCode) {
      return this.riksbankCurrencyCodes.has(currencyCode);
    }

    if (sourceCode === this.boeSourceCode) {
      return this.boeCurrencyCodes.has(currencyCode);
    }

    if (sourceCode === this.bocSourceCode) {
      return this.bocCurrencyCodes.has(currencyCode);
    }

    if (sourceCode === this.cnbSourceCode) {
      return this.cnbCurrencyCodes.has(currencyCode);
    }

    if (sourceCode === this.norgesSourceCode) {
      return this.norgesCurrencyCodes.has(currencyCode);
    }

    if (sourceCode === this.bnrSourceCode) {
      return this.bnrCurrencyCodes.has(currencyCode);
    }

    return false;
  }

  currencyOptionLabel(currency: { code: string; label: string }): string {
    const suffix = this.isCurrencyAvailableForSource(currency.code)
      ? ''
      : ' - brak danych dla zrodla';

    return `${currency.code} - ${currency.label}${suffix}`;
  }

  applyRangePreset(preset: { days?: number; ytd?: boolean; from?: string }): void {
    const today = new Date();
    this.to = this.formatDateInput(today);

    if (preset.from) {
      this.from = this.clampToMinimumRateDate(preset.from);
    } else if (preset.ytd) {
      this.from = this.clampToMinimumRateDate(`${today.getFullYear()}-01-01`);
    } else if (preset.days) {
      const startDate = new Date(today);
      startDate.setDate(startDate.getDate() - preset.days);
      this.from = this.clampToMinimumRateDate(this.formatDateInput(startDate));
    }

    void this.fetchData();
  }

  onDateRangeChange(): void {
    this.from = this.clampToMinimumRateDate(this.from);
    this.to = this.clampToMinimumRateDate(this.to);

    if (this.from > this.maximumRateDate) {
      this.from = this.maximumRateDate;
    }

    if (this.to > this.maximumRateDate) {
      this.to = this.maximumRateDate;
    }
  }

  private validateFilters(): boolean {
    if (!this.currency || !this.source || !this.from || !this.to) {
      this.errorMessage = 'Uzupelnij walute, zrodlo i zakres dat.';
      this.statusMessage = 'Wymagane sa wszystkie filtry.';
      return false;
    }

    if (this.from < this.minimumRateDate || this.to < this.minimumRateDate) {
      this.errorMessage = 'Zakres dat nie moze zaczynac sie przed 2026-01-01.';
      this.statusMessage = 'Wybierz date od 2026-01-01.';
      return false;
    }

    if (this.from > this.to) {
      this.errorMessage = 'Data poczatkowa nie moze byc pozniejsza niz koncowa.';
      this.statusMessage = 'Zakres dat wymaga poprawy.';
      return false;
    }

    return true;
  }

  private ensureSelectedCurrencyIsAvailable(): void {
    if (this.isCurrencyAvailableForSource(this.currency, this.source)) {
      return;
    }

    const firstAvailableCurrency = this.currencies.find((currency) =>
      this.isCurrencyAvailableForSource(currency.code, this.source),
    );

    if (firstAvailableCurrency) {
      this.currency = firstAvailableCurrency.code;
    }
  }

  private async loadDataFromBackend(): Promise<void> {
    const [chartResult, latestResult] = await Promise.allSettled([
      firstValueFrom(
        this.exchangeRateApi
          .getChartData(this.currency, this.source, this.from, this.to)
          .pipe(timeout(15000)),
      ),
      firstValueFrom(
        this.exchangeRateApi
          .getLatestRates(this.source)
          .pipe(timeout(15000)),
      ),
    ]);

    if (chartResult.status === 'fulfilled') {
      this.chartPoints = chartResult.value.points ?? [];
      this.tablePoints = [...this.chartPoints].sort((left, right) => right.date.localeCompare(left.date));
    } else {
      this.chartPoints = [];
      this.tablePoints = [];
      this.errorMessage = 'Nie udalo sie pobrac danych wykresu. Sprawdz backend albo zakres dat.';
    }

    if (latestResult.status === 'fulfilled') {
      this.latestRates = latestResult.value;
    } else {
      this.latestRates = [];
      this.errorMessage = this.errorMessage
        ? `${this.errorMessage} Nie udalo sie pobrac tabeli najnowszych kursow.`
        : 'Nie udalo sie pobrac tabeli najnowszych kursow.';
    }

    this.lastLoadedAt = new Date();
  }

  private shouldAutoSyncNbp(): boolean {
    return this.source === this.nbpSourceCode && !this.errorMessage && this.chartPoints.length === 0;
  }

  private finishFetchStatus(): void {
    if (!this.errorMessage) {
      this.statusMessage = this.chartPoints.length
        ? `Gotowe. Wczytano ${this.chartPoints.length} dni kursowych.`
        : 'Gotowe, ale backend nie zwrocil danych dla wybranych filtrow.';
    } else {
      this.statusMessage = 'Pobieranie zakonczone bledem.';
    }
  }

  private formatDateInput(date: Date): string {
    return date.toISOString().slice(0, 10);
  }

  private clampToMinimumRateDate(date: string): string {
    return date && date < this.minimumRateDate ? this.minimumRateDate : date;
  }

  private get chartValues(): number[] {
    return this.chartPoints.flatMap((point) => [point.buyPrice, point.sellPrice]);
  }

  private createLinePoints(field: 'buyPrice' | 'sellPrice'): string {
    if (this.chartPoints.length === 0) {
      return '';
    }

    const lastIndex = Math.max(this.chartPoints.length - 1, 1);

    return this.chartPoints
      .map((point, index) => {
        const x = this.getPointX(index);
        const y = this.getPointY(point[field]);

        return `${x.toFixed(2)},${y.toFixed(2)}`;
      })
      .join(' ');
  }

  private getPointX(index: number): number {
    const lastIndex = Math.max(this.chartPoints.length - 1, 1);
    return this.chartPadding.left + (this.plotWidth * index) / lastIndex;
  }

  private getPointY(value: number): number {
    const normalized = (value - this.chartMinValue) / this.chartRange;
    return this.chartPadding.top + this.plotHeight - normalized * this.plotHeight;
  }

}
