import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UserAlert } from '../models/alert.model';

@Injectable({
  providedIn: 'root',
})
export class AlertsService {
  private readonly apiUrl = '/api/Alerts';

  constructor(private readonly http: HttpClient) {}

  getUserAlerts(userId: number): Observable<UserAlert[]> {
    return this.http.get<UserAlert[]>(`${this.apiUrl}/user/${userId}`);
  }
}
