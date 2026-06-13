import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FavoriteCurrencyService } from '../../services/favorite-currency.service';
import { Observable } from 'rxjs';
//spprawdz i upewnij sie
//testowanie czy  railway sie nie popsuje

interface CurrencyExchangeSimulationResponse {
  exchangedAmount: number;
  feeAmount: number;
  finalAmount: number;
}

@Component({
  selector: 'app-currency-converter',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './currency-converter.html',
  styleUrls: ['./currency-converter.css'],
})
export class CurrencyConverterComponent implements OnInit {
  amount = 0;
  sourceCurrency = 'PLN';
  targetCurrency = 'USD';
  fee = 1;
  favorites: string[] = [];

  resultAmount: number | null = null;
  resultFee: number | null = null;
  resultTotal: number | null = null;

  readonly availableCurrencies: string[] = [
    'PLN',
    'EUR',
    'USD',
    'CHF',
    'GBP',
    'HUF',
    'CZK',
    'DKK',
    'SEK',
    'NOK',
    'RON',
    'TRY',
    'UAH',
    'AUD',
    'CAD',
    'JPY',
    'KRW',
  ];

  constructor(
    private readonly http: HttpClient,
    private readonly favoriteService: FavoriteCurrencyService,
  ) {}

  ngOnInit(): void {
    this.favoriteService.favorites$.subscribe({
      next: (data) => {
        this.favorites = data;
      },
      error: (error) => {
        console.error('Nie udalo sie pobrac ulubionych walut.', error);
      },
    });
  }

  isFavorite(currency: string): boolean {
    return this.favorites.includes(currency);
  }

  get sortedCurrencies(): string[] {
    return [...this.availableCurrencies].sort((a, b) => {
      const aFav = this.isFavorite(a);
      const bFav = this.isFavorite(b);

      if (aFav && !bFav) return -1;
      if (!aFav && bFav) return 1;

      return a.localeCompare(b);
    });
  }

  addToFavorites(currency: string): void {
    if (!currency || this.isFavorite(currency)) {
      return;
    }

    this.favoriteService.addFavorite(currency).subscribe({
      next: () => {
        // No need to update favorites here as it's handled by BehaviorSubject
      },
      error: (error) => {
        console.error('Nie udalo sie dodac ulubionej waluty.', error);
      },
    });
  }

  removeFromFavorites(currency: string): void {
    this.favoriteService.removeFavorite(currency).subscribe({
      next: () => {
        // No need to update favorites here as it's handled by BehaviorSubject
      },
      error: (error) => {
        console.error('Nie udalo sie usunac ulubionej waluty.', error);
      },
    });
  }

  calculateExchange(): void {
    if (this.amount <= 0 || !this.sourceCurrency || !this.targetCurrency) {
      alert('Prosze uzupelnic wszystkie pola poprawnie.');
      return;
    }

    this.http.post<CurrencyExchangeSimulationResponse>('/api/exchange/calculate', {
      amount: this.amount,
      sourceCurrency: this.sourceCurrency,
      targetCurrency: this.targetCurrency,
      feePercent: this.fee,
    }).subscribe({
      next: (response) => {
        this.resultAmount = response.exchangedAmount;
        this.resultFee = response.feeAmount;
        this.resultTotal = response.finalAmount;
      },
      error: (error) => {
        console.error('Nie udalo sie obliczyc wymiany.', error);
        alert(error.error?.message || 'Wystapil blad.');
      },
    });
  }
}