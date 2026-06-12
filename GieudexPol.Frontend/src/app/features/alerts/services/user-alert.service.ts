import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  AlertRateSource,
  UserAlertDto,
  UserAlertCreateDto,
  UserAlertUpdateDto,
} from '../../../shared/models/user-alert.model';

@Injectable({
  providedIn: 'root'
})
export class UserAlertService {
  private readonly apiUrl = '/api/UserAlerts';
  readonly hasUnacknowledgedAlerts = signal(false);

  constructor(private http: HttpClient) { }

  getMyAlerts(): Observable<UserAlertDto[]> {
    return this.http.get<UserAlertDto[]>(`${this.apiUrl}/me`).pipe(
      tap(alerts => this.hasUnacknowledgedAlerts.set(
        alerts.some(alert => !!alert.triggeredDate && !alert.isAcknowledged),
      )),
    );
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

  acknowledgeAlert(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/acknowledge`, {});
  }
}
