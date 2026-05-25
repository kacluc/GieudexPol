import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ExchangeRateDashboard } from './exchange-rate-dashboard/exchange-rate-dashboard';
import { AlertsComponent } from './features/alerts/components/alerts/alerts.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AuthGuard } from './features/auth/guards/auth.guard';
import { TransactionHistoryComponent } from './features/history/components/transaction-history/transaction-history.component';
import { OrderbookComponent } from './features/orderbook/components/orderbook/orderbook.component';
import { WalletManagementComponent } from './features/wallet/components/wallet-management/wallet-management.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { TransactionTransferComponent } from './transaction-transfer/transaction-transfer.component';

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
      { path: 'wallet', component: WalletManagementComponent },
      { path: 'transfer', component: TransactionTransferComponent },
      { path: 'history', component: TransactionHistoryComponent },
      { path: 'orderbook', component: OrderbookComponent },
      { path: 'alerts', component: AlertsComponent },
    ],
  },
  { path: '**', redirectTo: '' },
];
