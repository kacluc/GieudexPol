import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction } from '../models/transaction.model';


@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private apiUrl = '/api/Transactions';

  constructor(private http: HttpClient) { }

  createTransfer(transferRequest: { receiverUsername: string; amount: number; currencyId: number }): Observable<Transaction> {
    return this.http.post<Transaction>(`${this.apiUrl}/transfer`, transferRequest);
  }

  getUserTransactions(userId: number): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/user/${userId}`);
  }
}
