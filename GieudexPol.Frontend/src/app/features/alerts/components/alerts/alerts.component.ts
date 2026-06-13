import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CurrencyDto } from '../../../../models/currency.dto';
import { CurrencyService } from '../../../../services/currency.service';
import {
  AlertPriceSide,
  AlertRateSource,
  AlertStatus,
  AlertType,
  ThresholdDirection,
  TradingAlertEvent,
  TradingPairOption,
  UserAlertCreateDto,
  UserAlertDto,
  UserAlertUpdateDto,
  UserTradingAlertCreateDto,
  UserTradingAlertDto,
  UserTradingAlertUpdateDto,
} from '../../../../shared/models/user-alert.model';
import { AuthService } from '../../../auth/services/auth.service';
import { UserAlertService } from '../../services/user-alert.service';

type AlertMode = 'rates' | 'trading';

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
  readonly AlertStatus = AlertStatus;
  readonly TradingAlertEvent = TradingAlertEvent;
  readonly alertTypes = Object.values(AlertType);
  readonly priceSides = Object.values(AlertPriceSide);
  readonly tradingEvents = [
    TradingAlertEvent.SellOrder,
    TradingAlertEvent.BuyOrder,
    TradingAlertEvent.TradeExecution,
  ];
  readonly alerts = signal<UserAlertDto[]>([]);
  readonly tradingAlerts = signal<UserTradingAlertDto[]>([]);
  readonly currencies = signal<CurrencyDto[]>([]);
  readonly rateSources = signal<AlertRateSource[]>([]);
  readonly tradingPairs = signal<TradingPairOption[]>([]);
  readonly mode = signal<AlertMode>('rates');

  editingAlert: UserAlertDto | null = null;
  editingTradingAlert: UserTradingAlertDto | null = null;
  errorMessage = '';

  readonly alertForm;
  readonly tradingAlertForm;

  constructor(
    private readonly userAlertService: UserAlertService,
    private readonly formBuilder: FormBuilder,
    private readonly currencyService: CurrencyService,
    private readonly authService: AuthService,
    private readonly changeDetector: ChangeDetectorRef,
    private readonly router: Router,
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
    });
    this.tradingAlertForm = this.formBuilder.group({
      tradingPairId: [null as number | null, Validators.required],
      eventType: [TradingAlertEvent.SellOrder, Validators.required],
      direction: [ThresholdDirection.BelowOrEqual, Validators.required],
      targetPrice: [
        null as number | null,
        [Validators.required, Validators.min(0.0001)],
      ],
      minimumAmount: [null as number | null, Validators.min(0.0001)],
    });
  }

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      if (!user) {
        return;
      }

      this.loadAlerts();
      this.loadTradingAlerts();
      this.loadCurrencies();
      this.loadRateSources();
      this.loadTradingPairs();
    });

    this.alertForm.controls.alertType.valueChanges.subscribe(type => {
      this.updateValidators(type);
    });
    this.tradingAlertForm.controls.eventType.valueChanges.subscribe(event => {
      this.applyMarketDirection(event);
    });
  }

  get recentAlerts(): UserAlertDto[] {
    return this.alerts().slice(0, 3);
  }

  get recentTradingAlerts(): UserTradingAlertDto[] {
    return this.tradingAlerts().slice(0, 3);
  }

  setMode(mode: AlertMode): void {
    this.mode.set(mode);
    this.errorMessage = '';
    this.changeDetector.markForCheck();
  }

  loadAlerts(): void {
    this.userAlertService.getMyAlerts().subscribe({
      next: alerts => this.alerts.set(alerts),
      error: error => this.handleError(error, 'Nie udało się pobrać alertów kursowych.'),
    });
  }

  loadTradingAlerts(): void {
    this.userAlertService.getMyTradingAlerts().subscribe({
      next: alerts => this.tradingAlerts.set(alerts),
      error: error => this.handleError(error, 'Nie udało się pobrać alertów rynku.'),
    });
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

  loadTradingPairs(): void {
    this.userAlertService.getTradingPairs().subscribe({
      next: pairs => {
        this.tradingPairs.set(pairs);
        if (!this.tradingAlertForm.controls.tradingPairId.value && pairs[0]) {
          this.tradingAlertForm.controls.tradingPairId.setValue(pairs[0].id);
        }
      },
      error: error => this.handleError(error, 'Nie udało się pobrać par walutowych.'),
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
    if (this.mode() === 'trading') {
      this.submitTradingAlert();
      return;
    }

    this.submitRateAlert();
  }

  editAlert(alert: UserAlertDto): void {
    this.mode.set('rates');
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
    });
    this.updateValidators(alert.alertType);
  }

  editTradingAlert(alert: UserTradingAlertDto): void {
    this.mode.set('trading');
    this.editingTradingAlert = alert;
    this.tradingAlertForm.reset({
      tradingPairId: alert.tradingPairId,
      eventType: alert.eventType,
      direction: this.marketDirection(alert.eventType, alert.direction),
      targetPrice: alert.targetPrice,
      minimumAmount: alert.minimumAmount ?? null,
    });
  }

  deleteAlert(id: number): void {
    if (!confirm('Czy na pewno usunąć ten alert?')) {
      return;
    }

    this.userAlertService.deleteUserAlert(id).subscribe({
      next: () => this.loadAlerts(),
      error: error => this.handleError(error),
    });
  }

  deleteTradingAlert(id: number): void {
    if (!confirm('Czy na pewno usunąć ten alert rynku?')) {
      return;
    }

    this.userAlertService.deleteTradingAlert(id).subscribe({
      next: () => this.loadTradingAlerts(),
      error: error => this.handleError(error),
    });
  }

  resetForm(): void {
    if (this.mode() === 'trading') {
      this.resetTradingForm();
      return;
    }

    this.resetRateForm();
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

  tradingEventLabel(event: TradingAlertEvent): string {
    return {
      [TradingAlertEvent.BuyOrder]: 'Chcę sprzedać',
      [TradingAlertEvent.SellOrder]: 'Chcę kupić',
      [TradingAlertEvent.TradeExecution]: 'Wykonana transakcja',
    }[event];
  }

  marketDirectionLabel(event: TradingAlertEvent | null): string {
    return event === TradingAlertEvent.SellOrder
      ? '<= maksymalna cena zakupu'
      : '>= minimalna cena sprzedaży';
  }

  directionLabel(direction?: ThresholdDirection | null): string {
    return direction === ThresholdDirection.AboveOrEqual ? '>=' : '<=';
  }

  statusLabel(status: AlertStatus): string {
    return {
      [AlertStatus.Active]: 'Aktywny',
      [AlertStatus.Fulfilled]: 'Spełniony',
      [AlertStatus.Inactive]: 'Nieaktywny',
    }[status];
  }

  setRateAlertStatus(alert: UserAlertDto, status: AlertStatus): void {
    this.userAlertService.updateUserAlert(alert.id, {
      id: alert.id,
      currencyId: alert.currencyId,
      alertType: alert.alertType,
      priceSide: alert.priceSide,
      thresholdDirection: alert.thresholdDirection,
      rateSourceId: alert.rateSourceId,
      thresholdValue: alert.thresholdValue,
      percentageChange: alert.percentageChange,
      timeFrameHours: alert.timeFrameHours,
      status,
    }).subscribe({
      next: () => this.loadAlerts(),
      error: error => this.handleError(error),
    });
  }

  setTradingAlertStatus(
    alert: UserTradingAlertDto,
    status: AlertStatus,
  ): void {
    this.userAlertService.updateTradingAlert(alert.id, {
      id: alert.id,
      tradingPairId: alert.tradingPairId,
      eventType: alert.eventType,
      direction: alert.direction,
      targetPrice: alert.targetPrice,
      minimumAmount: alert.minimumAmount,
      status,
    }).subscribe({
      next: () => this.loadTradingAlerts(),
      error: error => this.handleError(error),
    });
  }

  openMarketOrder(alert: UserTradingAlertDto): void {
    if (alert.eventType === TradingAlertEvent.TradeExecution) {
      return;
    }

    void this.router.navigate(['/order-book'], {
      queryParams: {
        pairId: alert.tradingPairId,
        side: alert.eventType === TradingAlertEvent.SellOrder ? 'Buy' : 'Sell',
        price: alert.logs[0]?.currentPrice ?? alert.targetPrice,
      },
    });
  }

  private submitRateAlert(): void {
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
        status: this.editingAlert.status,
      };
      this.userAlertService.updateUserAlert(request.id, request).subscribe({
        next: () => {
          this.loadAlerts();
          this.resetRateForm();
        },
        error: error => this.handleError(error),
      });
      return;
    }

    const request: UserAlertCreateDto = common;
    this.userAlertService.createUserAlert(request).subscribe({
      next: () => {
        this.loadAlerts();
        this.resetRateForm();
      },
      error: error => this.handleError(error),
    });
  }

  private submitTradingAlert(): void {
    if (this.tradingAlertForm.invalid) {
      this.tradingAlertForm.markAllAsTouched();
      return;
    }

    const value = this.tradingAlertForm.getRawValue();
    if (
      value.tradingPairId == null ||
      value.eventType == null ||
      value.targetPrice == null
    ) {
      return;
    }

    const common = {
      tradingPairId: value.tradingPairId,
      eventType: value.eventType,
      direction: this.marketDirection(value.eventType, value.direction),
      targetPrice: value.targetPrice,
      minimumAmount: value.minimumAmount,
    };
    this.errorMessage = '';

    if (this.editingTradingAlert) {
      const request: UserTradingAlertUpdateDto = {
        id: this.editingTradingAlert.id,
        ...common,
        status: this.editingTradingAlert.status,
      };
      this.userAlertService.updateTradingAlert(request.id, request).subscribe({
        next: () => {
          this.loadTradingAlerts();
          this.resetTradingForm();
        },
        error: error => this.handleError(error),
      });
      return;
    }

    const request: UserTradingAlertCreateDto = common;
    this.userAlertService.createTradingAlert(request).subscribe({
      next: () => {
        this.loadTradingAlerts();
        this.resetTradingForm();
      },
      error: error => this.handleError(error),
    });
  }

  private resetRateForm(): void {
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
    });
  }

  private resetTradingForm(): void {
    this.editingTradingAlert = null;
    this.errorMessage = '';
    this.tradingAlertForm.reset({
      tradingPairId: this.tradingPairs()[0]?.id ?? null,
      eventType: TradingAlertEvent.SellOrder,
      direction: ThresholdDirection.BelowOrEqual,
      targetPrice: null,
      minimumAmount: null,
    });
  }

  private applyMarketDirection(event: TradingAlertEvent | null): void {
    if (event === TradingAlertEvent.SellOrder) {
      this.tradingAlertForm.controls.direction.setValue(
        ThresholdDirection.BelowOrEqual,
        { emitEvent: false },
      );
    } else if (event === TradingAlertEvent.BuyOrder) {
      this.tradingAlertForm.controls.direction.setValue(
        ThresholdDirection.AboveOrEqual,
        { emitEvent: false },
      );
    }
  }

  private marketDirection(
    event: TradingAlertEvent,
    direction: ThresholdDirection | null,
  ): ThresholdDirection {
    if (event === TradingAlertEvent.SellOrder) {
      return ThresholdDirection.BelowOrEqual;
    }

    if (event === TradingAlertEvent.BuyOrder) {
      return ThresholdDirection.AboveOrEqual;
    }

    return direction ?? ThresholdDirection.AboveOrEqual;
  }

  private handleError(
    error: { error?: { message?: string } },
    fallback = 'Nie udało się zapisać alertu.',
  ): void {
    this.errorMessage = error.error?.message ?? fallback;
    this.changeDetector.markForCheck();
  }
}
