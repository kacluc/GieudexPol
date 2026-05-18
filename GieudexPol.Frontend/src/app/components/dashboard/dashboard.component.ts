import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WalletService } from '../../features/wallet/services/wallet.service';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  userEmail = '';
  currentBalance: { [key: string]: number } = {};
  availableCurrencies: string[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  constructor(private walletService: WalletService, private authService: AuthService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  async loadDashboardData(): Promise<void> {
    this.userEmail = localStorage.getItem('userEmail') || 'Niewiadoma';
    this.isLoading = true;
    this.errorMessage = null;

    try {
      this.currentBalance = await this.walletService.getBalance();
      this.availableCurrencies = Object.keys(this.currentBalance ?? {});
    } catch (error) {
      console.error('Nie udalo sie zaladowac danych dashboardu:', error);
      this.currentBalance = {};
      this.availableCurrencies = [];
      this.errorMessage = 'Nie mozna zaladowac danych dashboardu. Sprawdz, czy API dziala na http://localhost:5265.';
    } finally {
      this.isLoading = false;
    }
  }

  logout(): void {
    this.authService.logout();
  }
}
