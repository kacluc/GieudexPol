import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { vi } from 'vitest';
import { CurrencyService } from '../../../../services/currency.service';
import { AuthService } from '../../../auth/services/auth.service';
import { UserAlertService } from '../../services/user-alert.service';
import { AlertsComponent } from './alerts.component';
import { Router } from '@angular/router';
import {
  AlertStatus,
  ThresholdDirection,
  TradingAlertEvent,
} from '../../../../shared/models/user-alert.model';

describe('AlertsComponent', () => {
  let fixture: ComponentFixture<AlertsComponent>;
  const router = { navigate: vi.fn() };
  const currencies$ = new Subject<
    Array<{ id: number; symbol: string; name: string; isActive: boolean }>
  >();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertsComponent],
      providers: [
        { provide: AuthService, useValue: { user$: of({ id: 1 }) } },
        { provide: Router, useValue: router },
        {
          provide: CurrencyService,
          useValue: { getAllCurrencies: () => currencies$.asObservable() },
        },
        {
          provide: UserAlertService,
          useValue: {
            getMyAlerts: () => of([]),
            getMyTradingAlerts: () => of([]),
            getRateSources: () => of([]),
            getTradingPairs: () => of([
              {
                id: 1,
                pair: 'EUR/PLN',
                baseCurrency: 'EUR',
                quoteCurrency: 'PLN',
                tickSize: 0.0001,
                isActive: true,
              },
            ]),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AlertsComponent);
    router.navigate.mockClear();
    fixture.detectChanges();
  });

  it('renders currencies immediately when the API response arrives', async () => {
    currencies$.next([
      { id: 1, symbol: 'PLN', name: 'Polski zloty', isActive: true },
      { id: 2, symbol: 'EUR', name: 'Euro', isActive: true },
    ]);
    await fixture.whenStable();

    const options = Array.from(
      fixture.nativeElement.querySelectorAll(
        '#currency option',
      ) as NodeListOf<HTMLOptionElement>,
    ).map(option => option.textContent?.trim());

    expect(options).toEqual([
      'Wybierz walutę',
      'PLN - Polski zloty',
      'EUR - Euro',
    ]);
  });

  it('shows a separate trading alert form with a preselected pair', async () => {
    const component = fixture.componentInstance;

    component.setMode('trading');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.tradingAlertForm.controls.tradingPairId.value).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Rynek');
    expect(fixture.nativeElement.textContent).toContain('Wykonana transakcja');
    expect(component.tradingAlertForm.controls.eventType.value)
      .toBe(TradingAlertEvent.SellOrder);
    expect(component.tradingAlertForm.controls.direction.value)
      .toBe(ThresholdDirection.BelowOrEqual);
  });

  it('sets the only meaningful direction for buying and selling intentions', () => {
    const component = fixture.componentInstance;

    component.tradingAlertForm.controls.eventType.setValue(
      TradingAlertEvent.BuyOrder,
    );
    expect(component.tradingAlertForm.controls.direction.value)
      .toBe(ThresholdDirection.AboveOrEqual);

    component.tradingAlertForm.controls.eventType.setValue(
      TradingAlertEvent.SellOrder,
    );
    expect(component.tradingAlertForm.controls.direction.value)
      .toBe(ThresholdDirection.BelowOrEqual);
  });

  it('opens the market with pair, operation and fulfilled offer price', () => {
    fixture.componentInstance.openMarketOrder({
      id: 7,
      tradingPairId: 1,
      pair: 'EUR/PLN',
      baseCurrency: 'EUR',
      quoteCurrency: 'PLN',
      eventType: TradingAlertEvent.SellOrder,
      direction: ThresholdDirection.BelowOrEqual,
      targetPrice: 4.3,
      status: AlertStatus.Fulfilled,
      createdDate: '2026-06-13T10:00:00Z',
      logs: [{
        id: 1,
        message: 'Oferta spełnia warunek',
        createdDate: '2026-06-13T10:05:00Z',
        currentPrice: 4.25,
      }],
    });

    expect(router.navigate).toHaveBeenCalledWith(['/order-book'], {
      queryParams: {
        pairId: 1,
        baseCurrency: 'EUR',
        quoteCurrency: 'PLN',
        side: 'Buy',
        price: 4.25,
      },
    });
  });
});
