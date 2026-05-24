import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FavoriteCurrencyService } from '../../services/favorite-currency.service';


@Component({
  selector: 'app-currency-converter',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './currency-converter.html',
  styleUrls: ['./currency-converter.css']
})
export class CurrencyConverterComponent implements OnInit {

  constructor(
    private http: HttpClient,
    private favoriteService: FavoriteCurrencyService
  ) { }

  amount: number = 0;
  sourceCurrency: string = 'PLN';
  targetCurrency: string = 'USD';
  fee: number = 1;
  favorites: string[] = [];

  resultAmount: number | null = null;
  resultFee: number | null = null;
  resultTotal: number | null = null;

  availableCurrencies: string[] = [
    'PLN',
    'USD',
    'EUR',
    'GBP',
    'JPY',
    'CHF'
  ];
  ngOnInit(): void {

    this.favoriteService.getFavorites()
      .subscribe({
        next: (data) => {
          this.favorites = data;
        },

        error: (err) => {
          console.error(err);
        }
      });
  }

  isFavorite(currency: string): boolean {
    return this.favorites.includes(currency);
  }

  addToFavorites(currency: string): void {

    this.favoriteService.addFavorite(currency)
      .subscribe({
        next: () => {
          this.favorites.push(currency);
        },

        error: (err) => {
          console.error(err);
        }
      });
  }

  removeFromFavorites(currency: string): void {

    this.favoriteService.removeFavorite(currency)
      .subscribe({
        next: () => {
          this.favorites =
            this.favorites.filter(x => x !== currency);
        },

        error: (err) => {
          console.error(err);
        }
      });
  }



  calculateExchange(): void {

    if (this.amount <= 0 || !this.sourceCurrency || !this.targetCurrency) {
      alert('Proszę uzupełnić wszystkie pola poprawnie.');
      return;
    }

    const request = {
      amount: this.amount,
      sourceCurrency: this.sourceCurrency,
      targetCurrency: this.targetCurrency,
      feePercent: this.fee
    };

    this.http.post<any>(
  'http://localhost:5265/api/exchange/calculate',
      request
    )
      .subscribe({
        next: (response) => {

          this.resultAmount = response.exchangedAmount;
          this.resultFee = response.feeAmount;
          this.resultTotal = response.finalAmount;

        },

        error: (err) => {
          console.error(err);
          alert(err.error?.message || 'Wystąpił błąd.');        }
      });
  }
}