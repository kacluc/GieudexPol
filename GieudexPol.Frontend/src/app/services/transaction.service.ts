import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction } from '../models/transaction.model';


@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private apiUrl = 'https://localhost:7071/api/transactions'; // Replace with your actual API URL

  constructor(private http: HttpClient) { }

  createTransfer(transferRequest: { senderId: number; receiverUsername: string; amount: number; currencyId: number }): Observable<Transaction> {
    return this.http.post<Transaction>(`${this.apiUrl}/transfer`, transferRequest);
  }

  getUserTransactions(userId: number): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/user/${userId}`);
  }
}
