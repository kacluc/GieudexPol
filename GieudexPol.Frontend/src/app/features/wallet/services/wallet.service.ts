import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { TradeRequest, TradeResponse, WalletDto } from '../models/wallet-models';

@Injectable({
  providedIn: 'root'
})
export class WalletService {
  private apiUrl = '/api/Wallets';

  constructor(private http: HttpClient) {}

  getUserWallets(userId: number): Observable<WalletDto[]> {
    return this.http.get<WalletDto[]>(`${this.apiUrl}/user/${userId}`);
  }

  async getBalance(userId = 1): Promise<{ [key: string]: number }> {
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

    return this.http.post<TradeResponse>(`${this.apiUrl}/trade?userId=${userId}`, request, { headers });
  }
}
