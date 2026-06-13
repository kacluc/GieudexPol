import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, finalize, firstValueFrom, map, of, shareReplay, switchMap, tap, timeout } from 'rxjs';
import {
  DepositRequest,
  ExchangePreviewRequest,
  ExchangePreviewResult,
  TradeRequest,
  TradeResponse,
  WalletCurrency,
  WalletDto,
  WithdrawRequest,
} from '../models/wallet-models';
import { AuthService } from '../../auth/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class WalletService {
  private static readonly requestTimeoutMs = 12_000;
  private apiUrl = '/api/Wallets';
  private readonly walletsSubject = new BehaviorSubject<WalletDto[]>([]);
  private loadedUserId: number | null = null;
  private loadingUserId: number | null = null;
  private activeUserId: number | null = null;
  private walletsRequest?: Observable<WalletDto[]>;

  readonly wallets$ = this.walletsSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
  ) {
    this.authService.user$.subscribe(user => {
      this.activeUserId = user?.id ?? null;
      if (!user) {
        this.clearWalletState();
        return;
      }

      if (this.loadedUserId !== user.id && this.loadingUserId !== user.id) {
        this.refreshWallets(user.id).subscribe({ error: () => undefined });
      }
    });
  }

  initialize(): void {
    // Instantiating this root service starts wallet preloading from the auth stream.
  }

  get walletSnapshot(): WalletDto[] {
    return this.walletsSubject.value;
  }

  getUserWallets(userId: number, forceRefresh = false): Observable<WalletDto[]> {
    if (!forceRefresh && this.loadedUserId === userId) {
      return of(this.walletsSubject.value);
    }

    if (this.loadingUserId === userId && this.walletsRequest) {
      return this.walletsRequest;
    }

    this.loadingUserId = userId;
    const request = this.http.get<WalletDto[]>(`${this.apiUrl}/user/${userId}`).pipe(
      timeout(WalletService.requestTimeoutMs),
      tap(wallets => {
        if (this.activeUserId === userId) {
          this.loadedUserId = userId;
          this.walletsSubject.next(wallets);
        }
      }),
      finalize(() => {
        if (this.loadingUserId === userId) {
          this.loadingUserId = null;
          this.walletsRequest = undefined;
        }
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    this.walletsRequest = request;
    return request;
  }

  refreshWallets(userId = this.loadedUserId ?? this.currentUserId()): Observable<WalletDto[]> {
    if (!userId) {
      return of([]);
    }

    return this.getUserWallets(userId, true);
  }

  getAvailableCurrencies(userId: number): Observable<WalletCurrency[]> {
    return this.http.get<WalletCurrency[]>(`${this.apiUrl}/available-currencies?userId=${userId}`);
  }

  addCurrencyWallet(userId: number, currencyId: number): Observable<WalletDto> {
    return this.http.post<WalletDto>(`${this.apiUrl}/user/${userId}/currencies/${currencyId}`, {}).pipe(
      switchMap(wallet => this.refreshWallets(userId).pipe(map(() => wallet))),
    );
  }

  async getBalance(userId: number): Promise<{ [key: string]: number }> {
    const wallets = await firstValueFrom(this.getUserWallets(userId));
    return wallets.reduce((balanceByCurrency, wallet) => {
      const symbol = wallet.currency?.symbol;
      if (symbol) {
        balanceByCurrency[symbol] = wallet.balance;
      }

      return balanceByCurrency;
    }, {} as { [key: string]: number });
  }

  executeTrade(userId: number, request: TradeRequest): Observable<TradeResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http.post<TradeResponse>(`${this.apiUrl}/trade?userId=${userId}`, request, { headers }).pipe(
      switchMap(response => this.refreshWallets(userId).pipe(map(() => response))),
    );
  }

  previewExchange(request: ExchangePreviewRequest): Observable<ExchangePreviewResult> {
    return this.http.post<ExchangePreviewResult>(
      '/api/wallet/exchange/preview',
      request,
    );
  }

  deposit(userId: number, request: DepositRequest): Observable<TradeResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http.post<TradeResponse>(`${this.apiUrl}/deposit?userId=${userId}`, request, { headers }).pipe(
      switchMap(response => this.refreshWallets(userId).pipe(map(() => response))),
    );
  }

  withdraw(userId: number, request: WithdrawRequest): Observable<TradeResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http.post<TradeResponse>(`${this.apiUrl}/withdraw?userId=${userId}`, request, { headers }).pipe(
      switchMap(response => this.refreshWallets(userId).pipe(map(() => response))),
    );
  }

  private currentUserId(): number | null {
    const userId = Number(localStorage.getItem('userId'));
    return Number.isInteger(userId) && userId > 0 ? userId : null;
  }

  private clearWalletState(): void {
    this.loadedUserId = null;
    this.loadingUserId = null;
    this.walletsRequest = undefined;
    this.walletsSubject.next([]);
  }
}
