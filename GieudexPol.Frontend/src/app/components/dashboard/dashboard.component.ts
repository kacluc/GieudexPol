import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WalletService } from '../../features/wallet/services/wallet.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  userEmail = '';
  currentBalance: { [key: string]: number } = {};
  availableCurrencies: string[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  readonly sections = [
    {
      title: 'Kursy walut',
      description: 'Wykres i dane kursowe z dostepnych zrodel.',
      route: '/rates',
      status: 'Dostepne',
    },
    {
      title: 'Portfel',
      description: 'Saldo, wymiana walut, wplaty i wyplaty.',
      route: '/wallet',
      status: 'Dostepne',
    },
    {
      title: 'Ulubione waluty',
      description: 'Symulator wymiany i lista wybranych walut.',
      route: '/converter',
      status: 'Dostepne',
    },
    {
      title: 'Transfer',
      description: 'Transfer srodkow pomiedzy uzytkownikami.',
      route: '/transfer',
      status: 'Dostepne',
    },
    {
      title: 'Historia transakcji',
      description: 'Rejestr operacji zapisanych dla uzytkownika.',
      route: '/history',
      status: 'Dostepne',
    },
    {
      title: 'Arkusz zleceń',
      description: 'Składanie zleceń oraz podgląd aktywnych ofert kupna i sprzedaży.',
      route: '/order-book',
      status: 'Dostępne',
    },
     {
       title: 'Alerty cenowe',
       description: 'Progi cenowe zapisane dla walut.',
       route: '/alerts',
       status: 'Dostepne',
     },
     {
       title: 'Ranking Waleni',
       description: 'Ranking najbogatszych uzytkownikow.',
       route: '/whale-ranking',
       status: 'Dostepne',
     },
  ];

  constructor(
    private readonly walletService: WalletService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    void this.loadDashboardData();
  }

  async loadDashboardData(): Promise<void> {
    this.userEmail = localStorage.getItem('userEmail') || 'Niewiadoma';
    this.isLoading = true;
    this.errorMessage = null;

    try {
      const userId = Number(localStorage.getItem('userId'));
      if (!Number.isInteger(userId) || userId <= 0) {
        throw new Error('Brak identyfikatora zalogowanego uzytkownika.');
      }

      this.currentBalance = await this.walletService.getBalance(userId);
      this.availableCurrencies = Object.keys(this.currentBalance ?? {});
    } catch (error) {
      console.error('Nie udalo sie zaladowac danych dashboardu:', error);
      this.currentBalance = {};
      this.availableCurrencies = [];
      this.errorMessage = 'Nie mozna zaladowac danych dashboardu. Sprawdz, czy API dziala na http://localhost:5265.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }
}
