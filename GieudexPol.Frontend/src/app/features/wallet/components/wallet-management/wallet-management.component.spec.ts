import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { AuthService } from '../../../auth/services/auth.service';
import { WalletService } from '../../services/wallet.service';
import { WalletManagementComponent } from './wallet-management.component';

describe('WalletManagementComponent', () => {
  let component: WalletManagementComponent;
  let fixture: ComponentFixture<WalletManagementComponent>;

  const walletService = {
    wallets$: of([
      { id: 1, currencyId: 1, balance: 100, currency: { id: 1, symbol: 'PLN' } },
      { id: 2, currencyId: 2, balance: 0, currency: { id: 2, symbol: 'EUR' } },
    ]),
    getUserWallets: vi.fn(),
    getAvailableCurrencies: vi.fn(),
    executeTrade: vi.fn(),
    previewExchange: vi.fn(),
  };

  beforeEach(async () => {
    walletService.getUserWallets.mockReturnValue(of([
      { id: 1, currencyId: 1, balance: 100, currency: { id: 1, symbol: 'PLN' } },
      { id: 2, currencyId: 2, balance: 0, currency: { id: 2, symbol: 'EUR' } },
    ]));
    walletService.getAvailableCurrencies.mockReturnValue(of([]));
    walletService.executeTrade.mockReturnValue(of({
      amountTo: 23.81,
      sellRateSource: 'PLN',
      buyRateSource: 'NBP',
      effectiveDate: '2026-05-25T00:00:00',
    }));
    walletService.previewExchange.mockReturnValue(of({
      fromCurrencyCode: 'PLN',
      toCurrencyCode: 'EUR',
      inputAmount: 100,
      estimatedOutputAmount: 23.81,
      rate: 0.2381,
      feeAmount: 10,
      feeCurrencyCode: 'PLN',
      totalDebitAmount: 110,
      rateDate: '2026-05-25T00:00:00',
      hasSufficientFunds: false,
      isPreview: true,
      message: 'To jest tylko symulacja.',
    }));

    await TestBed.configureTestingModule({
      imports: [WalletManagementComponent],
      providers: [
        { provide: WalletService, useValue: walletService },
        { provide: AuthService, useValue: { user$: of({ id: 1 }) } },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WalletManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('loads balances for user wallets', () => {
    expect(component.availableCurrencies).toEqual(['EUR', 'PLN']);
    expect(component.currentBalance['PLN']).toBe(100);
  });

  it('does not call trade endpoint for invalid amount', async () => {
    await component.executeTrade('PLN', 'EUR', 0);

    expect(walletService.executeTrade).not.toHaveBeenCalled();
    expect(component.tradeMessage).toContain('Kwota musi byc wieksza od zera');
  });

  it('executes exchange for currencies available in wallet', async () => {
    await component.executeTrade('PLN', 'EUR', 10);

    expect(walletService.executeTrade).toHaveBeenCalledWith(1, {
      fromCurrencyId: 1,
      amountFrom: 10,
      toCurrencyId: 2,
    });
    expect(component.tradeMessage).toContain('Sukces');
  });

  it('does not request preview for invalid amount', async () => {
    component.simulatorAmount = 0;

    await component.calculateExchangePreview();

    expect(walletService.previewExchange).not.toHaveBeenCalled();
    expect(component.simulatorError).toContain('większa od zera');
  });

  it('does not request preview for the same currency', async () => {
    component.simulatorFromCurrency = 'PLN';
    component.simulatorToCurrency = 'PLN';
    component.simulatorAmount = 100;

    await component.calculateExchangePreview();

    expect(walletService.previewExchange).not.toHaveBeenCalled();
    expect(component.simulatorError).toContain('muszą być różne');
  });

  it('shows preview result without executing real exchange', async () => {
    component.simulatorFromCurrency = 'PLN';
    component.simulatorToCurrency = 'EUR';
    component.simulatorAmount = 100;

    await component.calculateExchangePreview();

    expect(walletService.previewExchange).toHaveBeenCalledWith({
      fromCurrencyId: 1,
      toCurrencyId: 2,
      amount: 100,
    });
    expect(walletService.executeTrade).not.toHaveBeenCalled();
    expect(component.simulatorResult?.estimatedOutputAmount).toBe(23.81);
  });

  it('shows backend preview error', async () => {
    walletService.previewExchange.mockReturnValueOnce(throwError(() => ({
      error: { message: 'Brak źródła z wystarczającą płynnością.' },
    })));
    component.simulatorFromCurrency = 'PLN';
    component.simulatorToCurrency = 'EUR';
    component.simulatorAmount = 100;

    await component.calculateExchangePreview();

    expect(component.simulatorResult).toBeNull();
    expect(component.simulatorError).toContain('płynnością');
  });
});
