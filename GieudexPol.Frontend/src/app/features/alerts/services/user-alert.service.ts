import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, forkJoin, map, Observable, of, tap } from 'rxjs';
import {
  AlertRateSource,
  AlertStatus,
  TradingPairOption,
  UserAlertDto,
  UserAlertCreateDto,
  UserAlertUpdateDto,
  UserTradingAlertCreateDto,
  UserTradingAlertDto,
  UserTradingAlertUpdateDto,
} from '../../../shared/models/user-alert.model';

@Injectable({
  providedIn: 'root'
})
export class UserAlertService {
  private readonly apiUrl = '/api/UserAlerts';
  private readonly tradingApiUrl = '/api/trading-alerts';
  private hasFulfilledRateAlerts = false;
  private hasFulfilledTradingAlerts = false;
  readonly hasFulfilledAlerts = signal(false);

  constructor(private http: HttpClient) { }

  getMyAlerts(): Observable<UserAlertDto[]> {
    return this.http.get<UserAlertDto[]>(`${this.apiUrl}/me`).pipe(
      tap(alerts => {
        this.hasFulfilledRateAlerts = alerts.some(
          alert => alert.status === AlertStatus.Fulfilled,
        );
        this.updateNotificationState();
      }),
    );
  }

  getMyTradingAlerts(): Observable<UserTradingAlertDto[]> {
    return this.http.get<UserTradingAlertDto[]>(`${this.tradingApiUrl}/me`).pipe(
      tap(alerts => {
        this.hasFulfilledTradingAlerts = alerts.some(
          alert => alert.status === AlertStatus.Fulfilled,
        );
        this.updateNotificationState();
      }),
    );
  }

  refreshAlertIndicator(): Observable<void> {
    return forkJoin([
      this.getMyAlerts().pipe(
        catchError(() => {
          this.hasFulfilledRateAlerts = false;
          this.updateNotificationState();
          return of([] as UserAlertDto[]);
        }),
      ),
      this.getMyTradingAlerts().pipe(
        catchError(() => {
          this.hasFulfilledTradingAlerts = false;
          this.updateNotificationState();
          return of([] as UserTradingAlertDto[]);
        }),
      ),
    ]).pipe(map(() => undefined));
  }

  getTradingPairs(): Observable<TradingPairOption[]> {
    return this.http.get<TradingPairOption[]>('/api/trading-pairs');
  }

  getRateSources(): Observable<AlertRateSource[]> {
    return this.http.get<AlertRateSource[]>(`${this.apiUrl}/rate-sources`);
  }

  createUserAlert(userAlert: UserAlertCreateDto): Observable<UserAlertDto> {
    return this.http.post<UserAlertDto>(this.apiUrl, userAlert);
  }

  updateUserAlert(id: number, userAlert: UserAlertUpdateDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, userAlert);
  }

  deleteUserAlert(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  createTradingAlert(
    alert: UserTradingAlertCreateDto,
  ): Observable<UserTradingAlertDto> {
    return this.http.post<UserTradingAlertDto>(this.tradingApiUrl, alert);
  }

  updateTradingAlert(
    id: number,
    alert: UserTradingAlertUpdateDto,
  ): Observable<void> {
    return this.http.put<void>(`${this.tradingApiUrl}/${id}`, alert);
  }

  deleteTradingAlert(id: number): Observable<void> {
    return this.http.delete<void>(`${this.tradingApiUrl}/${id}`);
  }

  private updateNotificationState(): void {
    this.hasFulfilledAlerts.set(
      this.hasFulfilledRateAlerts || this.hasFulfilledTradingAlerts,
    );
  }
}
