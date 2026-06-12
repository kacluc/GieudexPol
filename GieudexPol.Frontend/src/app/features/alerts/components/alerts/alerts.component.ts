import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyDto } from '../../../../models/currency.dto';
import { CurrencyService } from '../../../../services/currency.service';
import {
  AlertPriceSide,
  AlertRateSource,
  AlertType,
  ThresholdDirection,
  UserAlertCreateDto,
  UserAlertDto,
  UserAlertUpdateDto,
} from '../../../../shared/models/user-alert.model';
import { AuthService } from '../../../auth/services/auth.service';
import { UserAlertService } from '../../services/user-alert.service';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.css'],
})
export class AlertsComponent implements OnInit {
  readonly AlertType = AlertType;
  readonly AlertPriceSide = AlertPriceSide;
  readonly ThresholdDirection = ThresholdDirection;
  readonly alertTypes = Object.values(AlertType);
  readonly priceSides = Object.values(AlertPriceSide);
  readonly alerts = signal<UserAlertDto[]>([]);
  readonly currencies = signal<CurrencyDto[]>([]);
  readonly rateSources = signal<AlertRateSource[]>([]);
  editingAlert: UserAlertDto | null = null;
  errorMessage = '';

  readonly alertForm;

  constructor(
    private readonly userAlertService: UserAlertService,
    private readonly formBuilder: FormBuilder,
    private readonly currencyService: CurrencyService,
    private readonly authService: AuthService,
  ) {
    this.alertForm = this.formBuilder.group({
      currencyId: [null as number | null, Validators.required],
      alertType: [null as AlertType | null, Validators.required],
      priceSide: [AlertPriceSide.UserBuysCurrency, Validators.required],
      rateSourceId: [null as number | null],
      thresholdValue: [null as number | null],
      thresholdDirection: [null as ThresholdDirection | null],
      percentageChange: [null as number | null],
      timeFrameHours: [24 as number | null],
      isActive: [true, Validators.required],
    });
  }

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      if (user) {
        this.loadAlerts();
        this.loadCurrencies();
        this.loadRateSources();
      }
    });

    this.alertForm.controls.alertType.valueChanges.subscribe(type => {
      this.updateValidators(type);
    });
  }

  loadAlerts(): void {
    this.userAlertService.getMyAlerts().subscribe({
      next: alerts => this.alerts.set(alerts),
      error: () => this.errorMessage = 'Nie udalo sie pobrac alertow.',
    });
  }

  get recentAlerts(): UserAlertDto[] {
    return this.alerts().slice(0, 3);
  }

  loadCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe(currencies => {
      this.currencies.set(currencies);
    });
  }

  loadRateSources(): void {
    this.userAlertService.getRateSources().subscribe(sources => {
      this.rateSources.set(sources);
    });
  }

  updateValidators(alertType: AlertType | null): void {
    const thresholdValue = this.alertForm.controls.thresholdValue;
    const thresholdDirection = this.alertForm.controls.thresholdDirection;
    const percentageChange = this.alertForm.controls.percentageChange;
    const timeFrameHours = this.alertForm.controls.timeFrameHours;

    thresholdValue.clearValidators();
    thresholdDirection.clearValidators();
    percentageChange.clearValidators();
    timeFrameHours.clearValidators();

    if (alertType === AlertType.Threshold) {
      thresholdValue.setValidators([Validators.required, Validators.min(0.0001)]);
      thresholdDirection.setValidators(Validators.required);
    } else if (
      alertType === AlertType.PriceDrop ||
      alertType === AlertType.PriceIncrease
    ) {
      percentageChange.setValidators([
        Validators.required,
        Validators.min(0.0001),
      ]);
      timeFrameHours.setValidators([Validators.required, Validators.min(1)]);
    }

    thresholdValue.updateValueAndValidity();
    thresholdDirection.updateValueAndValidity();
    percentageChange.updateValueAndValidity();
    timeFrameHours.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.alertForm.invalid) {
      this.alertForm.markAllAsTouched();
      return;
    }

    const value = this.alertForm.getRawValue();
    if (value.currencyId == null || value.alertType == null || value.priceSide == null) {
      return;
    }

    const common = {
      currencyId: value.currencyId,
      alertType: value.alertType,
      priceSide: value.priceSide,
      rateSourceId: value.rateSourceId,
      thresholdValue:
        value.alertType === AlertType.Threshold ? value.thresholdValue : null,
      thresholdDirection:
        value.alertType === AlertType.Threshold ? value.thresholdDirection : null,
      percentageChange:
        value.alertType === AlertType.Threshold ? null : value.percentageChange,
      timeFrameHours:
        value.alertType === AlertType.Threshold ? null : value.timeFrameHours,
    };

    this.errorMessage = '';
    if (this.editingAlert) {
      const request: UserAlertUpdateDto = {
        id: this.editingAlert.id,
        ...common,
        isActive: value.isActive ?? true,
      };
      this.userAlertService.updateUserAlert(request.id, request).subscribe({
        next: () => {
          this.loadAlerts();
          this.resetForm();
        },
        error: error => this.handleError(error),
      });
      return;
    }

    const request: UserAlertCreateDto = common;
    this.userAlertService.createUserAlert(request).subscribe({
      next: () => {
        this.loadAlerts();
        this.resetForm();
      },
      error: error => this.handleError(error),
    });
  }

  editAlert(alert: UserAlertDto): void {
    this.editingAlert = alert;
    this.alertForm.reset({
      currencyId: alert.currencyId,
      alertType: alert.alertType,
      priceSide: alert.priceSide,
      rateSourceId: alert.rateSourceId ?? null,
      thresholdValue: alert.thresholdValue ?? null,
      thresholdDirection: alert.thresholdDirection ?? null,
      percentageChange: alert.percentageChange ?? null,
      timeFrameHours: alert.timeFrameHours ?? 24,
      isActive: alert.isActive,
    });
    this.updateValidators(alert.alertType);
  }

  deleteAlert(id: number): void {
    if (!confirm('Czy na pewno usunac ten alert?')) {
      return;
    }

    this.userAlertService.deleteUserAlert(id).subscribe({
      next: () => this.loadAlerts(),
      error: error => this.handleError(error),
    });
  }

  acknowledgeAlert(id: number): void {
    this.userAlertService.acknowledgeAlert(id).subscribe({
      next: () => this.loadAlerts(),
      error: error => this.handleError(
        error,
        'Nie udało się potwierdzić alertu.',
      ),
    });
  }

  resetForm(): void {
    this.editingAlert = null;
    this.errorMessage = '';
    this.alertForm.reset({
      currencyId: null,
      alertType: null,
      priceSide: AlertPriceSide.UserBuysCurrency,
      rateSourceId: null,
      thresholdValue: null,
      thresholdDirection: null,
      percentageChange: null,
      timeFrameHours: 24,
      isActive: true,
    });
  }

  alertTypeLabel(type: AlertType): string {
    return {
      [AlertType.PriceIncrease]: 'Wzrost ceny',
      [AlertType.PriceDrop]: 'Spadek ceny',
      [AlertType.Threshold]: 'Próg cenowy',
    }[type];
  }

  priceSideLabel(side: AlertPriceSide): string {
    return {
      [AlertPriceSide.UserBuysCurrency]: 'Kupna',
      [AlertPriceSide.UserSellsCurrency]: 'Sprzedaży',
      [AlertPriceSide.MidPrice]: 'Kurs średni',
    }[side];
  }

  directionLabel(direction?: ThresholdDirection | null): string {
    return direction === ThresholdDirection.AboveOrEqual
      ? '>='
      : '<=';
  }

  statusLabel(alert: UserAlertDto): string {
    if (alert.isAcknowledged) {
      return 'Przyjęty';
    }

    return alert.triggeredDate ? 'Spełniony' : 'Aktywny';
  }

  private handleError(
    error: { error?: { message?: string } },
    fallback = 'Nie udało się zapisać alertu.',
  ): void {
    this.errorMessage =
      error.error?.message ?? fallback;
  }
}
