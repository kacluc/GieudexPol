import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';
import { WalletService } from '../../services/wallet.service';
import {
  DepositRequest,
  ExchangePreviewResult,
  TradeRequest,
  WalletCurrency,
  WalletDto,
  WithdrawRequest,
} from '../../models/wallet-models';

@Component({
  selector: 'app-wallet-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './wallet-management.component.html',
  styleUrls: ['./wallet-management.component.scss']
})
export class WalletManagementComponent implements OnInit, OnDestroy {
  private readonly developmentUserId = 1;
  private currentUserId = this.developmentUserId;

  fromCurrency = 'PLN';
  toCurrency = 'EUR';
  amount: number | null = null;
  depositCurrency = 'PLN';
  depositAmount: number | null = null;
  withdrawCurrency = 'PLN';
  withdrawAmount: number | null = null;
  newCurrencyId: number | null = null;
  simulatorFromCurrency = 'PLN';
  simulatorToCurrency = 'EUR';
  simulatorAmount: number | null = null;
  simulatorResult: ExchangePreviewResult | null = null;
  simulatorError = '';
  simulatorLoading = false;

  availableCurrencies: string[] = [];
  addableCurrencies: WalletCurrency[] = [];
  currentBalance: { [key: string]: number } = {};
  wallets: WalletDto[] = [];
  isLoading = false;
  tradeMessage: string | null = null;
  activeTab: 'exchange' | 'deposit' | 'withdraw' = 'exchange';
  private readonly subscriptions = new Subscription();

  constructor(
    private walletService: WalletService,
    private router: Router,
    private authService: AuthService,
    private changeDetector: ChangeDetectorRef
  ) {}

  setActiveTab(tab: 'exchange' | 'deposit' | 'withdraw'): void {
    this.activeTab = tab;
  }

  async ngOnInit(): Promise<void> {
    this.subscriptions.add(
      this.authService.user$.subscribe(user => {
        if (user?.id) {
          this.currentUserId = user.id;
        }
      }),
    );
    this.subscriptions.add(
      this.walletService.wallets$.subscribe(wallets => {
        if (wallets.length > 0) {
          this.applyWallets(wallets);
          this.changeDetector.detectChanges();
        }
      }),
    );

    await this.initializeBalance();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  async initializeBalance(): Promise<void> {
    this.isLoading = true;

    try {
      const wallets = await firstValueFrom(this.walletService.getUserWallets(this.currentUserId));
      this.applyWallets(wallets);
      this.changeDetector.detectChanges();

      this.addableCurrencies = await firstValueFrom(
        this.walletService.getAvailableCurrencies(this.currentUserId),
      );
      this.newCurrencyId = this.addableCurrencies[0]?.id ?? null;
      this.ensureSimulatorCurrencies();
    } catch (error) {
      console.error('Nie udalo sie zaladowac salda:', error);
      this.tradeMessage = 'Blad: Nie mozna zaladowac danych portfela.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  async addCurrencyWallet(): Promise<void> {
    if (this.newCurrencyId === null) {
      return;
    }

    const currency = this.addableCurrencies.find(item => item.id === this.newCurrencyId);
    this.isLoading = true;
    this.tradeMessage = null;

    try {
      await firstValueFrom(this.walletService.addCurrencyWallet(this.currentUserId, this.newCurrencyId));
      this.tradeMessage = `Sukces: dodano portfel ${currency?.symbol ?? ''}.`;
      await this.initializeBalance();
    } catch (error) {
      console.error('Blad dodawania waluty:', error);
      this.tradeMessage = 'Blad: Nie udalo sie dodac wybranej waluty do portfela.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  async executeTrade(fromCurrency: string, toCurrency: string, amount: number | null): Promise<void> {
    if (amount === null || amount <= 0) {
      this.tradeMessage = 'Kwota musi byc wieksza od zera.';
      return;
    }

    if (fromCurrency === toCurrency) {
      this.tradeMessage = 'Wybierz dwie rozne waluty.';
      return;
    }

    const fromWallet = this.findWallet(fromCurrency);
    const toWallet = this.findWallet(toCurrency);
    if (!fromWallet || !toWallet) {
      this.tradeMessage = 'Brak portfela dla wybranej waluty.';
      return;
    }

    if ((this.currentBalance[fromCurrency] ?? 0) < amount) {
      this.tradeMessage = 'Brak wystarczajacych srodkow w portfelu zrodlowym.';
      return;
    }

    const tradeRequest: TradeRequest = {
      fromCurrencyId: fromWallet.currencyId,
      amountFrom: amount,
      toCurrencyId: toWallet.currencyId
    };

    this.isLoading = true;
    this.tradeMessage = null;

    try {
      const response = await firstValueFrom(this.walletService.executeTrade(this.currentUserId, tradeRequest));
      const amountTo = response.amountTo ?? 0;
      const rateDate = response.effectiveDate?.slice(0, 10) ?? 'brak daty';
      const fee = response.feeAmount ?? 0;
      const feeCurrency = response.feeCurrency ?? fromCurrency;
      this.tradeMessage =
        `Sukces: wymieniono ${amount.toFixed(2)} ${fromCurrency} na ${amountTo.toFixed(2)} ${toCurrency} ` +
        `(prowizja: ${fee.toFixed(2)} ${feeCurrency}, kurs z ${rateDate}).`;
      this.amount = null;
      await this.initializeBalance();
    } catch (error) {
      console.error('Blad transakcji:', error);
      this.tradeMessage = 'Blad: Nie udalo sie wykonac transakcji. Sprawdz srodki albo dostepnosc kursu.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  async executeDeposit(): Promise<void> {
    if (this.depositAmount === null || this.depositAmount <= 0) {
      this.tradeMessage = 'Kwota wplaty musi byc wieksza od zera.';
      return;
    }

    const wallet = this.findWallet(this.depositCurrency);
    if (!wallet) {
      this.tradeMessage = 'Brak portfela dla wybranej waluty.';
      return;
    }

    const request: DepositRequest = { currencyId: wallet.currencyId, amount: this.depositAmount };
    this.isLoading = true;
    this.tradeMessage = null;

    try {
      await firstValueFrom(this.walletService.deposit(this.currentUserId, request));
      this.tradeMessage = `Sukces: wplacono ${this.depositAmount.toFixed(2)} ${this.depositCurrency}.`;
      this.depositAmount = null;
      await this.initializeBalance();
    } catch (error) {
      console.error('Blad wplaty:', error);
      this.tradeMessage = 'Blad: Nie udalo sie wykonac wplaty.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  async executeWithdraw(): Promise<void> {
    if (this.withdrawAmount === null || this.withdrawAmount <= 0) {
      this.tradeMessage = 'Kwota wyplaty musi byc wieksza od zera.';
      return;
    }

    const wallet = this.findWallet(this.withdrawCurrency);
    if (!wallet) {
      this.tradeMessage = 'Brak portfela dla wybranej waluty.';
      return;
    }

    if ((this.currentBalance[this.withdrawCurrency] ?? 0) < this.withdrawAmount) {
      this.tradeMessage = 'Brak wystarczajacych srodkow do wyplaty.';
      return;
    }

    const request: WithdrawRequest = { currencyId: wallet.currencyId, amount: this.withdrawAmount };
    this.isLoading = true;
    this.tradeMessage = null;

    try {
      await firstValueFrom(this.walletService.withdraw(this.currentUserId, request));
      this.tradeMessage = `Sukces: wyplacono ${this.withdrawAmount.toFixed(2)} ${this.withdrawCurrency}.`;
      this.withdrawAmount = null;
      await this.initializeBalance();
    } catch (error) {
      console.error('Blad wyplaty:', error);
      this.tradeMessage = 'Blad: Nie udalo sie wykonac wyplaty.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  navigateToTransfer(): void {
    this.router.navigate(['/transfer']);
  }

  get simulatorCurrencies(): WalletCurrency[] {
    const currencies = [
      ...this.wallets
        .map(wallet => wallet.currency)
        .filter((currency): currency is WalletCurrency => !!currency),
      ...this.addableCurrencies,
    ];

    return Array.from(
      new Map(currencies.map(currency => [currency.id, currency])).values(),
    ).sort((left, right) => left.symbol.localeCompare(right.symbol));
  }

  async calculateExchangePreview(): Promise<void> {
    this.simulatorError = '';
    this.simulatorResult = null;

    if (this.simulatorAmount === null || this.simulatorAmount <= 0) {
      this.simulatorError = 'Kwota musi być większa od zera.';
      return;
    }

    if (this.simulatorFromCurrency === this.simulatorToCurrency) {
      this.simulatorError = 'Waluta źródłowa i docelowa muszą być różne.';
      return;
    }

    const fromCurrency = this.findSimulatorCurrency(this.simulatorFromCurrency);
    const toCurrency = this.findSimulatorCurrency(this.simulatorToCurrency);
    if (!fromCurrency || !toCurrency) {
      this.simulatorError = 'Nie znaleziono wybranej waluty.';
      return;
    }

    this.simulatorLoading = true;
    try {
      this.simulatorResult = await firstValueFrom(
        this.walletService.previewExchange({
          fromCurrencyId: fromCurrency.id,
          toCurrencyId: toCurrency.id,
          amount: this.simulatorAmount,
        }),
      );
    } catch (error) {
      const httpError = error as HttpErrorResponse;
      this.simulatorError =
        httpError.error?.message ??
        'Nie udało się obliczyć symulacji. Sprawdź dostępność kursu i płynności.';
    } finally {
      this.simulatorLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  private findWallet(symbol: string): WalletDto | undefined {
    return this.wallets.find(wallet => wallet.currency?.symbol === symbol);
  }

  private applyWallets(wallets: WalletDto[]): void {
    this.wallets = wallets;
    this.currentBalance = {};
    this.availableCurrencies = wallets
      .map(wallet => wallet.currency?.symbol)
      .filter((symbol): symbol is string => !!symbol)
      .sort();

    for (const wallet of wallets) {
      const symbol = wallet.currency?.symbol;
      if (symbol) {
        this.currentBalance[symbol] = wallet.availableBalance ?? wallet.balance;
      }
    }

    this.fromCurrency = this.ensureSelectedCurrency(this.fromCurrency);
    this.toCurrency = this.ensureSelectedCurrency(
      this.toCurrency,
      this.availableCurrencies.find(symbol => symbol !== this.fromCurrency) ?? this.fromCurrency,
    );
    this.depositCurrency = this.ensureSelectedCurrency(this.depositCurrency);
    this.withdrawCurrency = this.ensureSelectedCurrency(this.withdrawCurrency);
    this.ensureSimulatorCurrencies();
  }

  private ensureSimulatorCurrencies(): void {
    const symbols = this.simulatorCurrencies.map(currency => currency.symbol);
    this.simulatorFromCurrency = symbols.includes(this.simulatorFromCurrency)
      ? this.simulatorFromCurrency
      : (symbols[0] ?? '');
    this.simulatorToCurrency =
      symbols.includes(this.simulatorToCurrency) &&
      this.simulatorToCurrency !== this.simulatorFromCurrency
        ? this.simulatorToCurrency
        : (symbols.find(symbol => symbol !== this.simulatorFromCurrency) ??
          this.simulatorFromCurrency);
  }

  private findSimulatorCurrency(symbol: string): WalletCurrency | undefined {
    return this.simulatorCurrencies.find(currency => currency.symbol === symbol);
  }

  private ensureSelectedCurrency(value: string, fallback?: string): string {
    return this.availableCurrencies.includes(value)
      ? value
      : (fallback ?? this.availableCurrencies[0] ?? '');
  }
}
