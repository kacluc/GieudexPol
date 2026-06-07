import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserAlertDto, UserAlertCreateDto, UserAlertUpdateDto } from '../../../shared/models/user-alert.model';

@Injectable({
  providedIn: 'root'
})
export class UserAlertService {
  private readonly apiUrl = '/api/UserAlerts';

  constructor(private http: HttpClient) { }

  getUserAlerts(userId: number): Observable<UserAlertDto[]> {
    return this.http.get<UserAlertDto[]>(`${this.apiUrl}/user/${userId}`);
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
}
