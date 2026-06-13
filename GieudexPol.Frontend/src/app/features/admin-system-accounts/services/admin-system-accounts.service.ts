import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminSystemAccount } from '../models/admin-system-account.model';

@Injectable({ providedIn: 'root' })
export class AdminSystemAccountsService {
  private readonly apiUrl = '/api/admin/system-accounts';

  constructor(private readonly http: HttpClient) {}

  getAccounts(): Observable<AdminSystemAccount[]> {
    return this.http.get<AdminSystemAccount[]>(this.apiUrl);
  }
}
