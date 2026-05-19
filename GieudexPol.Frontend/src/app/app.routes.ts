import { Routes } from '@angular/router';
import { AuthGuard } from './features/auth/guards/auth.guard';
 
// Import zaawansowanego komponentu logowania z dedykowanego folderu
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
 
import { WalletManagementComponent } from './features/wallet/components/wallet-management/wallet-management.component';
// Import komponentu Dashboard (założenie ścieżki)
import { DashboardComponent } from './components/dashboard/dashboard.component'; 
import { ExchangeRateDashboard } from './exchange-rate-dashboard/exchange-rate-dashboard';
import { TransactionTransferComponent } from './transaction-transfer/transaction-transfer.component';
 
export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  // Trasa dla Dashboardu - główny widok po zalogowaniu
  { path: 'auth/login', component: LoginComponent }, 
  { path: 'auth/register', component: RegisterComponent },
  // Publiczna trasa testowa do widoku kursow walut i synchronizacji NBP
  { path: 'rates', component: ExchangeRateDashboard },
  // Nowa trasa deweloperska do testów widoku portfela (bez autoryzacji)
  { path: 'test-wallet', component: WalletManagementComponent }, 
  // Trasa dla Dashboardu - główny widok po zalogowaniu (wymaga autoryzacji)
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] }, 
  // Dodanie trasy dla zarządzania portfelem (MUSI zachować AuthGuard!)
  { path: 'wallet', component: WalletManagementComponent, canActivate: [AuthGuard] }, 
  { path: 'transfer', component: TransactionTransferComponent, canActivate: [AuthGuard] },
  // ... inne istniejące trasy
];
