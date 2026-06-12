import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, finalize, of, retry, shareReplay, tap, timeout } from 'rxjs';
import { AuthService } from '../../auth/services/auth.service';
import {
  CreateOrderRequest,
  OrderBook,
  TradingPair,
  UserOrder,
} from '../models/order-book.model';

@Injectable({ providedIn: 'root' })
export class OrderBookService {
  private static readonly requestTimeoutMs = 12_000;
  private readonly pairsSubject = new BehaviorSubject<TradingPair[]>([]);
  private readonly myOrdersSubject = new BehaviorSubject<UserOrder[]>([]);
  private pairsRequest?: Observable<TradingPair[]>;
  private ordersRequest?: Observable<UserOrder[]>;
  private ordersUserId: number | null = null;

  readonly tradingPairs$ = this.pairsSubject.asObservable();
  readonly myOrders$ = this.myOrdersSubject.asObservable();

  constructor(
    private readonly http: HttpClient,
    private readonly authService: AuthService,
  ) {
    this.authService.user$.subscribe(user => {
      if (this.ordersUserId !== user?.id) {
        this.ordersUserId = user?.id ?? null;
        this.myOrdersSubject.next([]);
        this.ordersRequest = undefined;
      }

      if (user && this.pairsSubject.value.length === 0) {
        this.getTradingPairs().subscribe({ error: () => undefined });
      }
    });
  }

  initialize(): void {
    this.getTradingPairs().subscribe({ error: () => undefined });
  }

  getTradingPairs(forceRefresh = false): Observable<TradingPair[]> {
    if (!forceRefresh && this.pairsSubject.value.length > 0) {
      return of(this.pairsSubject.value);
    }

    if (this.pairsRequest) {
      return this.pairsRequest;
    }

    const request = this.http.get<TradingPair[]>('/api/trading-pairs').pipe(
      timeout(OrderBookService.requestTimeoutMs),
      retry({ count: 2, delay: 400 }),
      tap(pairs => this.pairsSubject.next(pairs)),
      finalize(() => {
        this.pairsRequest = undefined;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    this.pairsRequest = request;
    return request;
  }

  getOrderBook(pair: TradingPair, depth = 15): Observable<OrderBook> {
    const params = new HttpParams()
      .set('baseCurrencyCode', pair.baseCurrency)
      .set('quoteCurrencyCode', pair.quoteCurrency)
      .set('depth', depth);

    return this.http.get<OrderBook>('/api/order-book', { params }).pipe(
      timeout(OrderBookService.requestTimeoutMs),
    );
  }

  getMyOrders(forceRefresh = false): Observable<UserOrder[]> {
    if (!forceRefresh && this.myOrdersSubject.value.length > 0) {
      return of(this.myOrdersSubject.value);
    }

    if (this.ordersRequest) {
      return this.ordersRequest;
    }

    const request = this.http.get<UserOrder[]>('/api/orders/my').pipe(
      timeout(OrderBookService.requestTimeoutMs),
      retry({ count: 1, delay: 300 }),
      tap(orders => this.myOrdersSubject.next(orders)),
      finalize(() => {
        this.ordersRequest = undefined;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    this.ordersRequest = request;
    return request;
  }

  createOrder(request: CreateOrderRequest): Observable<UserOrder> {
    return this.http.post<UserOrder>('/api/orders', request).pipe(
      timeout(OrderBookService.requestTimeoutMs),
    );
  }

  cancelOrder(orderId: number): Observable<void> {
    return this.http.delete<void>(`/api/orders/${orderId}/cancel`).pipe(
      timeout(OrderBookService.requestTimeoutMs),
    );
  }
}
