import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AdminUser,
  AdminUserRole,
  CreateAdminUser,
} from '../models/admin-user.model';

@Injectable({
  providedIn: 'root',
})
export class AdminUsersService {
  private readonly apiUrl = '/api/admin/users';

  constructor(private readonly http: HttpClient) {}

  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(this.apiUrl);
  }

  getUser(id: number): Observable<AdminUser> {
    return this.http.get<AdminUser>(`${this.apiUrl}/${id}`);
  }

  createUser(request: CreateAdminUser): Observable<AdminUser> {
    return this.http.post<AdminUser>(this.apiUrl, request);
  }

  updateUserRole(id: number, role: AdminUserRole): Observable<AdminUser> {
    return this.http.put<AdminUser>(`${this.apiUrl}/${id}/role`, { role });
  }

  resetPassword(id: number, newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/reset-password`, { newPassword });
  }
}
