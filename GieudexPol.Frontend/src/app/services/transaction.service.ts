import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction } from '../models/transaction.model';
import { PaginatedResult } from '../models/paginated-result.model';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private apiUrl = '/api/Transactions';

  constructor(private http: HttpClient) { }

  createTransfer(transferRequest: { receiverUsername: string; amount: number; currencyId: number }): Observable<Transaction> {
    return this.http.post<Transaction>(`${this.apiUrl}/transfer`, transferRequest);
  }

  getUserTransactions(
    userId: number,
    pageNumber: number = 1,
    pageSize: number = 10,
    transactionType: string | null = null,
    currencyId: number | null = null,
    startDate: string | null = null,
    endDate: string | null = null
  ): Observable<PaginatedResult<Transaction>> {
    let params = new HttpParams();
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    if (transactionType) {
      params = params.append('transactionType', transactionType);
    }
    if (currencyId) {
      params = params.append('currencyId', currencyId.toString());
    }
    if (startDate) {
      params = params.append('startDate', startDate);
    }
    if (endDate) {
      params = params.append('endDate', endDate);
    }

    return this.http.get<PaginatedResult<Transaction>>(`${this.apiUrl}/user/${userId}`, { params });
  }
}
