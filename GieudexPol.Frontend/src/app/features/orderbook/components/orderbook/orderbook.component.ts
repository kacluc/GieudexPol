import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize, interval, Subscription, TimeoutError } from 'rxjs';
import { WalletDto } from '../../../wallet/models/wallet-models';
import { WalletService } from '../../../wallet/services/wallet.service';
import {
  OrderBook,
  OrderSide,
  TradingPair,
  UserOrder,
} from '../../models/order-book.model';
import { OrderBookService } from '../../services/order-book.service';

@Component({
  selector: 'app-orderbook',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './orderbook.component.html',
  styleUrl: './orderbook.component.scss',
})
export class OrderbookComponent implements OnInit, OnDestroy {
  pairs: TradingPair[] = [];
  selectedPair?: TradingPair;
  orderBook?: OrderBook;
  myOrders: UserOrder[] = [];
  wallets: WalletDto[] = [];
  side: OrderSide = 'Buy';
  price: number | null = null;
  amount: number | null = null;
  loading = true;
  refreshing = false;
  submitting = false;
  message = '';
  error = '';

  private readonly subscriptions = new Subscription();
  private refreshPending = false;

  constructor(
    private readonly orderBookService: OrderBookService,
    private readonly walletService: WalletService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.subscriptions.add(
      this.walletService.wallets$.subscribe(wallets => {
        this.wallets = [...wallets].sort((left, right) =>
          (left.currency?.symbol ?? '').localeCompare(right.currency?.symbol ?? ''),
        );
        this.render();
      }),
    );
    this.subscriptions.add(
      this.orderBookService.myOrders$.subscribe(orders => {
        this.myOrders = orders;
        this.render();
      }),
    );
    this.subscriptions.add(
      this.orderBookService.tradingPairs$.subscribe(pairs => this.applyPairs(pairs)),
    );
    this.subscriptions.add(
      this.orderBookService.getTradingPairs().subscribe({
        error: error => {
          this.loading = false;
          this.error = this.readError(error, 'Nie udało się pobrać par walutowych.');
          this.render();
        },
      }),
    );
    this.subscriptions.add(
      interval(5_000).subscribe(() => this.refresh(false)),
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  get activeOrders(): UserOrder[] {
    return this.myOrders.filter(
      order => order.status === 'Open' || order.status === 'PartiallyFilled',
    );
  }

  get orderDescription(): string {
    if (!this.selectedPair) {
      return 'Wybierz parę walutową.';
    }

    return this.side === 'Buy'
      ? `Kupno: kupujesz ${this.selectedPair.baseCurrency} za ${this.selectedPair.quoteCurrency}.`
      : `Sprzedaż: sprzedajesz ${this.selectedPair.baseCurrency} za ${this.selectedPair.quoteCurrency}.`;
  }

  selectPair(pairId: string): void {
    const pair = this.pairs.find(item => item.id === Number(pairId));
    if (!pair || pair.id === this.selectedPair?.id) {
      return;
    }

    this.selectedPair = pair;
    this.price = null;
    this.amount = null;
    this.orderBook = undefined;
    this.refresh();
  }

  refresh(showLoader = true): void {
    if (!this.selectedPair) {
      return;
    }

    if (this.refreshing) {
      this.refreshPending = true;
      return;
    }

    const refreshedPair = this.selectedPair;
    this.refreshing = true;
    if (showLoader && !this.orderBook) {
      this.loading = true;
    }

    let pendingRequests = 3;
    const errors: string[] = [];
    const finishRequest = (): void => {
      pendingRequests -= 1;
      if (pendingRequests === 0) {
        this.refreshing = false;
        this.loading = false;
        this.error = errors.join(' ');
        this.render();

        if (this.refreshPending) {
          this.refreshPending = false;
          this.refresh(false);
        }
      }
    };

    this.subscriptions.add(
      this.orderBookService.getOrderBook(refreshedPair).subscribe({
        next: orderBook => {
          if (this.selectedPair?.id === refreshedPair.id) {
            this.orderBook = orderBook;
          }
        },
        error: error => {
          errors.push(this.readError(error, 'Nie udało się odświeżyć ofert.'));
          finishRequest();
        },
        complete: finishRequest,
      }),
    );
    this.subscriptions.add(
      this.orderBookService.getMyOrders(true).subscribe({
        error: error => {
          errors.push(this.readError(error, 'Nie udało się odświeżyć aktywnych zleceń.'));
          finishRequest();
        },
        complete: finishRequest,
      }),
    );
    this.subscriptions.add(
      this.walletService.refreshWallets().subscribe({
        error: error => {
          errors.push(this.readError(error, 'Nie udało się odświeżyć stanu portfela.'));
          finishRequest();
        },
        complete: finishRequest,
      }),
    );
  }

  submitOrder(): void {
    if (
      !this.selectedPair ||
      this.price == null ||
      this.amount == null ||
      this.price <= 0 ||
      this.amount <= 0
    ) {
      this.error = 'Cena i ilość muszą być większe od zera.';
      return;
    }

    this.submitting = true;
    this.error = '';
    this.message = '';
    this.render();
    this.orderBookService.createOrder({
        baseCurrencyCode: this.selectedPair.baseCurrency,
        quoteCurrencyCode: this.selectedPair.quoteCurrency,
        side: this.side,
        price: this.price,
        amount: this.amount,
      })
      .pipe(
        finalize(() => {
          this.submitting = false;
          this.render();
        }),
      )
      .subscribe({
      next: order => {
        this.price = null;
        this.amount = null;
        this.message = order.status === 'Filled'
          ? 'Zlecenie zostało wykonane.'
          : 'Zlecenie zostało przyjęte do arkusza.';
        this.refresh(false);
      },
      error: error => {
        this.error = this.readError(error, 'Nie udało się złożyć zlecenia.');
        this.render();
      },
    });
  }

  cancelOrder(order: UserOrder): void {
    this.error = '';
    this.message = '';
    this.orderBookService.cancelOrder(order.id).subscribe({
      next: () => {
        this.message = 'Zlecenie zostało anulowane, a środki odblokowane.';
        this.refresh(false);
        this.render();
      },
      error: error => {
        this.error = this.readError(error, 'Nie udało się anulować zlecenia.');
        this.render();
      },
    });
  }

  sideLabel(side: OrderSide): string {
    return side === 'Buy' ? 'Kupno' : 'Sprzedaż';
  }

  statusLabel(status: UserOrder['status']): string {
    const labels: Record<UserOrder['status'], string> = {
      Open: 'Aktywne',
      PartiallyFilled: 'Częściowo wykonane',
      Filled: 'Wykonane',
      Cancelled: 'Anulowane',
    };

    return labels[status];
  }

  private applyPairs(pairs: TradingPair[]): void {
    this.pairs = pairs;
    if (pairs.length === 0) {
      return;
    }

    const selectedPair = pairs.find(pair => pair.id === this.selectedPair?.id) ?? pairs[0];
    const changed = selectedPair.id !== this.selectedPair?.id;
    this.selectedPair = selectedPair;

    if (changed || !this.orderBook) {
      this.refresh();
    }

    this.render();
  }

  private readError(error: unknown, fallback: string): string {
    if (error instanceof TimeoutError) {
      return 'Serwer nie odpowiedział w ciągu 12 sekund. Sprawdź, czy backend działa na porcie 5265.';
    }

    const response = error as { error?: { message?: string; error?: string } };
    return response.error?.message ?? response.error?.error ?? fallback;
  }

  private render(): void {
    this.changeDetector.markForCheck();
  }
}
