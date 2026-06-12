import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { CurrencyConverterComponent } from './currency-exchange/currency-converter/currency-converter';
import { ExchangeRateDashboard } from './exchange-rate-dashboard/exchange-rate-dashboard';
import { AlertsComponent } from './features/alerts/components/alerts/alerts.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AuthGuard } from './features/auth/guards/auth.guard';
import { TransactionHistoryComponent } from './features/history/components/transaction-history/transaction-history.component';
import { WalletManagementComponent } from './features/wallet/components/wallet-management/wallet-management.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { TransactionTransferComponent } from './transaction-transfer/transaction-transfer.component';
import { WhaleRankingListComponent } from './features/whale-ranking/components/whale-ranking-list.component';
import { AdminUsersComponent } from './features/admin-users/components/admin-users/admin-users.component';
import { AdminGuard } from './features/auth/guards/admin.guard';

export const routes: Routes = [
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', component: DashboardComponent },
      { path: 'dashboard', redirectTo: '', pathMatch: 'full' },
      { path: 'rates', component: ExchangeRateDashboard },
      { path: 'converter', component: CurrencyConverterComponent },
      { path: 'wallet', component: WalletManagementComponent },
      { path: 'transfer', component: TransactionTransferComponent },
      { path: 'history', component: TransactionHistoryComponent },
      {
        path: 'order-book',
        loadComponent: () =>
          import('./features/orderbook/components/orderbook/orderbook.component')
            .then(module => module.OrderbookComponent),
      },
      { path: 'orderbook', redirectTo: 'order-book', pathMatch: 'full' },
      { path: 'alerts', component: AlertsComponent },
      { path: 'whale-ranking', component: WhaleRankingListComponent },
      {
        path: 'admin/users',
        component: AdminUsersComponent,
        canActivate: [AdminGuard],
      },
      {
        path: 'admin/test-exchange-rates',
        loadComponent: () =>
          import(
            './features/admin-test-exchange-rates/components/admin-test-exchange-rates/admin-test-exchange-rates.component'
          ).then(module => module.AdminTestExchangeRatesComponent),
        canActivate: [AdminGuard],
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
