import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { WalletService } from '../../services/wallet.service';
import { TradeRequest, WalletDto } from '../../models/wallet-models';

@Component({
  selector: 'app-wallet-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './wallet-management.component.html',
  styleUrls: ['./wallet-management.component.scss']
})
export class WalletManagementComponent implements OnInit {
  private readonly developmentUserId = 1;
  private readonly ratesToPln: Record<string, number> = {
    PLN: 1,
    EUR: 4.3,
    USD: 4.0,
    CHF: 4.55,
    GBP: 5.05
  };

  fromCurrency = 'PLN';
  toCurrency = 'EUR';
  amount: number | null = null;

  availableCurrencies: string[] = [];
  currentBalance: { [key: string]: number } = {};
  wallets: WalletDto[] = [];
  isLoading = false;
  tradeMessage: string | null = null;

  constructor(private walletService: WalletService) {}

  async ngOnInit(): Promise<void> {
    await this.initializeBalance();
  }

  async initializeBalance(): Promise<void> {
    this.isLoading = true;

    try {
      this.wallets = await firstValueFrom(this.walletService.getUserWallets(this.developmentUserId));
      this.currentBalance = {};
      this.availableCurrencies = this.wallets
        .map(wallet => wallet.currency?.symbol)
        .filter((symbol): symbol is string => !!symbol)
        .sort();

      for (const wallet of this.wallets) {
        const symbol = wallet.currency?.symbol;
        if (symbol) {
          this.currentBalance[symbol] = wallet.balance;
        }
      }

      if (!this.availableCurrencies.includes(this.fromCurrency)) {
        this.fromCurrency = this.availableCurrencies[0] ?? '';
      }

      if (!this.availableCurrencies.includes(this.toCurrency)) {
        this.toCurrency = this.availableCurrencies.find(symbol => symbol !== this.fromCurrency) ?? '';
      }
    } catch (error) {
      console.error('Nie udalo sie zaladowac salda:', error);
      this.tradeMessage = 'Blad: Nie mozna zaladowac danych portfela.';
    } finally {
      this.isLoading = false;
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

    const amountTo = this.calculateTargetAmount(fromCurrency, toCurrency, amount);
    const tradeRequest: TradeRequest = {
      fromCurrencyId: fromWallet.currencyId,
      amountFrom: amount,
      toCurrencyId: toWallet.currencyId,
      amountTo
    };

    this.isLoading = true;
    this.tradeMessage = null;

    try {
      await firstValueFrom(this.walletService.executeTrade(this.developmentUserId, tradeRequest));
      this.tradeMessage = `Sukces: wymieniono ${amount.toFixed(2)} ${fromCurrency} na ${amountTo.toFixed(2)} ${toCurrency}.`;
      this.amount = null;
      await this.initializeBalance();
    } catch (error) {
      console.error('Blad transakcji:', error);
      this.tradeMessage = 'Blad: Nie udalo sie wykonac transakcji. Sprawdz srodki albo polaczenie z API.';
    } finally {
      this.isLoading = false;
    }
  }

  private findWallet(symbol: string): WalletDto | undefined {
    return this.wallets.find(wallet => wallet.currency?.symbol === symbol);
  }

  private calculateTargetAmount(fromCurrency: string, toCurrency: string, amount: number): number {
    const fromRate = this.ratesToPln[fromCurrency] ?? 1;
    const toRate = this.ratesToPln[toCurrency] ?? 1;
    return Math.round((amount * fromRate / toRate) * 100) / 100;
  }
}
