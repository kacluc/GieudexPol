import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { WalletService } from '../../../wallet/services/wallet.service';
import { OrderBook, TradingPair, UserOrder } from '../../models/order-book.model';
import { OrderBookService } from '../../services/order-book.service';
import { OrderbookComponent } from './orderbook.component';

describe('OrderbookComponent', () => {
  let component: OrderbookComponent;
  let fixture: ComponentFixture<OrderbookComponent>;

  const pair: TradingPair = {
    id: 1,
    pair: 'EUR/PLN',
    baseCurrency: 'EUR',
    quoteCurrency: 'PLN',
    tickSize: 0.0001,
    isActive: true,
  };
  const orderBook: OrderBook = {
    pair: 'EUR/PLN',
    baseCurrency: 'EUR',
    quoteCurrency: 'PLN',
    buyOrders: [],
    sellOrders: [],
  };
  const activeOrder: UserOrder = {
    id: 7,
    pair: 'EUR/PLN',
    baseCurrency: 'EUR',
    quoteCurrency: 'PLN',
    side: 'Buy',
    type: 'Limit',
    status: 'Open',
    price: 4.2,
    originalAmount: 10,
    remainingAmount: 10,
    createdAt: '2026-06-12T10:00:00Z',
  };

  const pairsSubject = new BehaviorSubject<TradingPair[]>([pair]);
  const ordersSubject = new BehaviorSubject<UserOrder[]>([]);
  const walletsSubject = new BehaviorSubject([
    {
      id: 1,
      userId: 1,
      currencyId: 1,
      currency: { id: 1, symbol: 'PLN', name: 'Polski złoty', isActive: true },
      balance: 1000,
      reservedBalance: 0,
      availableBalance: 1000,
    },
  ]);
  const orderBookService = {
    tradingPairs$: pairsSubject.asObservable(),
    myOrders$: ordersSubject.asObservable(),
    getTradingPairs: vi.fn(),
    getOrderBook: vi.fn(),
    getMyOrders: vi.fn(),
    createOrder: vi.fn(),
    cancelOrder: vi.fn(),
  };
  const walletService = {
    wallets$: walletsSubject.asObservable(),
    refreshWallets: vi.fn(),
  };

  beforeEach(async () => {
    pairsSubject.next([pair]);
    ordersSubject.next([]);
    orderBookService.getTradingPairs.mockReturnValue(of([pair]));
    orderBookService.getOrderBook.mockReturnValue(of(orderBook));
    orderBookService.getMyOrders.mockImplementation(() => {
      ordersSubject.next([activeOrder]);
      return of([activeOrder]);
    });
    orderBookService.createOrder.mockReturnValue(of(activeOrder));
    orderBookService.cancelOrder.mockReturnValue(of(undefined));
    walletService.refreshWallets.mockReturnValue(of(walletsSubject.value));

    await TestBed.configureTestingModule({
      imports: [OrderbookComponent],
      providers: [
        { provide: OrderBookService, useValue: orderBookService },
        { provide: WalletService, useValue: walletService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderbookComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    fixture.destroy();
    vi.clearAllMocks();
  });

  it('selects the first pair and loads the current order book immediately', () => {
    fixture.detectChanges();

    expect(component.selectedPair).toEqual(pair);
    expect(component.orderBook).toEqual(orderBook);
    expect(component.activeOrders).toEqual([activeOrder]);
    expect(component.wallets[0].balance).toBe(1000);
  });

  it('keeps active orders visible when refreshing order book levels fails', () => {
    orderBookService.getOrderBook.mockReturnValue(
      throwError(() => ({ error: { message: 'Błąd ofert' } })),
    );

    fixture.detectChanges();

    expect(component.activeOrders).toEqual([activeOrder]);
    expect(component.error).toContain('Błąd ofert');
  });

  it('refreshes wallet balances after creating an order', () => {
    fixture.detectChanges();
    walletService.refreshWallets.mockClear();
    component.price = 4.2;
    component.amount = 10;

    component.submitOrder();

    expect(orderBookService.createOrder).toHaveBeenCalled();
    expect(walletService.refreshWallets).toHaveBeenCalled();
  });
});
