import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { CurrencyDto } from '../../../../models/currency.dto';
import { CurrencyService } from '../../../../services/currency.service';
import {
  AdminTestExchangeRate,
  AdminTestRateSource,
  AlertEvaluationRequest,
  CreateTestExchangeRate,
  UpdateTestExchangeRate,
} from '../../models/admin-test-exchange-rate.model';
import { AdminTestExchangeRatesService } from '../../services/admin-test-exchange-rates.service';

@Component({
  selector: 'app-admin-test-exchange-rates',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './admin-test-exchange-rates.component.html',
  styleUrl: './admin-test-exchange-rates.component.scss',
})
export class AdminTestExchangeRatesComponent implements OnInit {
  readonly defaultSourceCode = 'MOCK_BANK_A';
  rates: AdminTestExchangeRate[] = [];
  sources: AdminTestRateSource[] = [];
  currencies: CurrencyDto[] = [];
  editingRate: AdminTestExchangeRate | null = null;
  loading = false;
  saving = false;
  evaluatingAlerts = false;
  errorMessage = '';
  successMessage = '';

  readonly filterForm;
  readonly rateForm;

  constructor(
    private readonly adminRatesService: AdminTestExchangeRatesService,
    private readonly currencyService: CurrencyService,
    private readonly formBuilder: FormBuilder,
    private readonly changeDetector: ChangeDetectorRef,
  ) {
    this.filterForm = this.formBuilder.group({
      rateSourceCode: [this.defaultSourceCode, Validators.required],
      currencyId: [null as number | null],
      dateFrom: [''],
      dateTo: [''],
    });

    this.rateForm = this.formBuilder.group({
      rateSourceCode: [this.defaultSourceCode, Validators.required],
      currencyId: [null as number | null, Validators.required],
      effectiveDate: ['', Validators.required],
      buyPrice: [null as number | null, [Validators.required, Validators.min(0.0001)]],
      sellPrice: [null as number | null, [Validators.required, Validators.min(0.0001)]],
      midPrice: [null as number | null, Validators.min(0.0001)],
    });
  }

  ngOnInit(): void {
    this.loadSources();
    this.loadCurrencies();
    this.loadRates();
  }

  get selectedSourceName(): string {
    const sourceCode = this.filterForm.controls.rateSourceCode.value;
    return this.sources.find(source => source.code === sourceCode)?.name ?? sourceCode ?? '';
  }

  get maxEffectiveDate(): string {
    return this.toDateInput(this.yesterday());
  }

  loadRates(): void {
    const filters = this.filterForm.getRawValue();
    this.loading = true;
    this.errorMessage = '';

    this.adminRatesService.getTestExchangeRates({
      rateSourceCode: filters.rateSourceCode || undefined,
      currencyId: filters.currencyId ?? undefined,
      dateFrom: filters.dateFrom || undefined,
      dateTo: filters.dateTo || undefined,
    }).subscribe({
      next: rates => {
        this.rates = rates;
        this.loading = false;
        this.changeDetector.markForCheck();
      },
      error: error => this.handleError(error),
    });
  }

  clearFilters(): void {
    this.filterForm.reset({
      rateSourceCode: this.defaultSourceCode,
      currencyId: null,
      dateFrom: '',
      dateTo: '',
    });
    this.loadRates();
  }

  onSourceChanged(): void {
    const sourceCode =
      this.filterForm.controls.rateSourceCode.value ?? this.defaultSourceCode;
    this.rateForm.controls.rateSourceCode.setValue(sourceCode);
    if (this.editingRate?.rateSourceCode !== sourceCode) {
      this.startAdding();
    }
    this.loadRates();
  }

  startAdding(): void {
    this.editingRate = null;
    this.rateForm.controls.currencyId.enable();
    this.rateForm.controls.rateSourceCode.enable();
    const selectedSourceCode =
      this.filterForm.controls.rateSourceCode.value ?? this.defaultSourceCode;
    this.rateForm.reset({
      rateSourceCode: selectedSourceCode,
      currencyId: null,
      effectiveDate: this.toDateInput(this.yesterday()),
      buyPrice: null,
      sellPrice: null,
      midPrice: null,
    });
    this.clearMessages();
  }

  startEditing(rate: AdminTestExchangeRate): void {
    this.editingRate = rate;
    this.rateForm.reset({
      rateSourceCode: rate.rateSourceCode,
      currencyId: rate.currencyId,
      effectiveDate: this.toDateInput(rate.effectiveDate),
      buyPrice: rate.buyPrice,
      sellPrice: rate.sellPrice,
      midPrice: rate.midPrice,
    });
    this.rateForm.controls.currencyId.disable();
    this.rateForm.controls.rateSourceCode.disable();
    this.clearMessages();
  }

  cancelEditing(): void {
    this.startAdding();
  }

  saveRate(evaluateAfterSave = false): void {
    if (this.rateForm.invalid) {
      this.rateForm.markAllAsTouched();
      return;
    }

    const value = this.rateForm.getRawValue();
    if (value.buyPrice == null || value.sellPrice == null || !value.effectiveDate) {
      return;
    }

    if (value.sellPrice < value.buyPrice) {
      this.errorMessage = 'Cena sprzedazy nie moze byc nizsza od ceny kupna.';
      return;
    }

    this.saving = true;
    this.clearMessages();

    if (this.editingRate) {
      const request: UpdateTestExchangeRate = {
        effectiveDate: value.effectiveDate,
        buyPrice: value.buyPrice,
        sellPrice: value.sellPrice,
        midPrice: value.midPrice ?? undefined,
      };
      this.finishSave(
        this.adminRatesService.updateTestExchangeRate(this.editingRate.id, request),
        'Testowy kurs zostal zaktualizowany.',
        evaluateAfterSave,
      );
      return;
    }

    if (value.currencyId == null || !value.rateSourceCode) {
      this.rateForm.controls.currencyId.setErrors({ required: true });
      this.saving = false;
      return;
    }

    const request: CreateTestExchangeRate = {
      rateSourceCode: value.rateSourceCode,
      currencyId: value.currencyId,
      effectiveDate: value.effectiveDate,
      buyPrice: value.buyPrice,
      sellPrice: value.sellPrice,
      midPrice: value.midPrice ?? undefined,
    };
    this.finishSave(
      this.adminRatesService.createTestExchangeRate(request),
      'Testowy kurs zostal dodany.',
      evaluateAfterSave,
    );
  }

  evaluateAlertsNow(): void {
    const formValue = this.rateForm.getRawValue();
    const currency = this.currencies.find(item => item.id === formValue.currencyId);
    const request: AlertEvaluationRequest = {
      currencyCode: currency?.symbol,
      rateSourceCode:
        formValue.rateSourceCode ??
        this.filterForm.controls.rateSourceCode.value ??
        undefined,
    };
    this.runAlertEvaluation(request);
  }

  deleteRate(rate: AdminTestExchangeRate): void {
    const confirmed = confirm(
      `Czy usunac testowy kurs ${rate.currencyCode} z dnia ${this.toDateInput(rate.effectiveDate)}?`,
    );
    if (!confirmed) {
      return;
    }

    this.saving = true;
    this.clearMessages();
    this.adminRatesService.deleteTestExchangeRate(rate.id).subscribe({
      next: () => {
        this.saving = false;
        if (this.editingRate?.id === rate.id) {
          this.startAdding();
        }
        this.successMessage = 'Testowy kurs zostal usuniety.';
        this.loadRates();
      },
      error: error => this.handleError(error),
    });
  }

  private loadCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe({
      next: currencies => {
        this.currencies = [...currencies].sort((left, right) =>
          left.symbol.localeCompare(right.symbol),
        );
        this.changeDetector.markForCheck();
      },
      error: error => this.handleError(error),
    });
  }

  private loadSources(): void {
    this.adminRatesService.getTestRateSources().subscribe({
      next: sources => {
        this.sources = sources;
        const selectedCode = this.filterForm.controls.rateSourceCode.value;
        if (!sources.some(source => source.code === selectedCode) && sources.length > 0) {
          this.filterForm.controls.rateSourceCode.setValue(sources[0].code);
          this.rateForm.controls.rateSourceCode.setValue(sources[0].code);
          this.loadRates();
        }
        this.changeDetector.markForCheck();
      },
      error: error => this.handleError(error),
    });
  }

  private finishSave(
    request: Observable<AdminTestExchangeRate>,
    successMessage: string,
    evaluateAfterSave: boolean,
  ): void {
    request.subscribe({
      next: savedRate => {
        this.successMessage = successMessage;
        this.saving = false;
        this.startAdding();
        this.successMessage = successMessage;
        this.loadRates();
        if (evaluateAfterSave) {
          this.runAlertEvaluation({
            currencyCode: savedRate.currencyCode,
            rateSourceCode: savedRate.rateSourceCode,
          });
        }
      },
      error: error => this.handleError(error),
    });
  }

  private clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  private handleError(error: HttpErrorResponse): void {
    this.loading = false;
    this.saving = false;
    this.evaluatingAlerts = false;

    if (error.status === 401 || error.status === 403) {
      this.errorMessage = 'Brak uprawnien administratora lub operacja dotyczy chronionego zrodla.';
    } else if (error.status === 400) {
      this.errorMessage = this.readErrorMessage(error) ?? 'Nieprawidlowe dane kursu.';
    } else if (error.status === 404) {
      this.errorMessage = this.readErrorMessage(error) ?? 'Kurs lub developmentowe zrodlo nie istnieje.';
    } else if (error.status === 409) {
      this.errorMessage = this.readErrorMessage(error) ?? 'Kurs dla tej waluty i daty juz istnieje.';
    } else {
      this.errorMessage = this.readErrorMessage(error) ?? 'Nie udalo sie wykonac operacji.';
    }

    this.changeDetector.markForCheck();
  }

  private runAlertEvaluation(request: AlertEvaluationRequest): void {
    this.evaluatingAlerts = true;
    this.clearMessages();
    this.adminRatesService.evaluateAlerts(request).subscribe({
      next: result => {
        this.evaluatingAlerts = false;
        this.successMessage =
          `Sprawdzono ${result.evaluatedAlertsCount} alertow, ` +
          `odpalono ${result.triggeredAlertsCount}, ` +
          `utworzono ${result.notificationsCreatedCount} powiadomien.`;
        this.changeDetector.markForCheck();
      },
      error: error => this.handleError(error),
    });
  }

  private readErrorMessage(error: HttpErrorResponse): string | null {
    if (typeof error.error === 'string') {
      return error.error;
    }
    if (error.error?.message) {
      return error.error.message;
    }
    if (error.error?.errors) {
      return Object.values(error.error.errors).flat().join(' ');
    }
    return null;
  }

  private toDateInput(value: string): string {
    return value.slice(0, 10);
  }

  private yesterday(): string {
    const date = new Date();
    date.setDate(date.getDate() - 1);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
