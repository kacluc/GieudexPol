import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminSystemAccount } from '../../models/admin-system-account.model';
import { AdminSystemAccountsService } from '../../services/admin-system-accounts.service';

@Component({
  selector: 'app-admin-system-accounts',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './admin-system-accounts.component.html',
  styleUrl: './admin-system-accounts.component.scss',
})
export class AdminSystemAccountsComponent implements OnInit {
  accounts: AdminSystemAccount[] = [];
  selectedUserId: number | null = null;
  loading = false;
  errorMessage = '';

  constructor(
    private readonly systemAccountsService: AdminSystemAccountsService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  get selectedAccount(): AdminSystemAccount | null {
    return this.accounts.find(account => account.userId === this.selectedUserId) ?? null;
  }

  get rateSourceAccountsCount(): number {
    return this.accounts.filter(account => account.accountType === 'RateSourceSystem').length;
  }

  get treasury(): AdminSystemAccount | undefined {
    return this.accounts.find(account => account.accountType === 'PlatformTreasury');
  }

  loadAccounts(): void {
    this.loading = true;
    this.errorMessage = '';
    this.systemAccountsService.getAccounts().subscribe({
      next: accounts => {
        this.accounts = accounts;
        if (!accounts.some(account => account.userId === this.selectedUserId)) {
          this.selectedUserId = accounts[0]?.userId ?? null;
        }
        this.loading = false;
        this.changeDetector.markForCheck();
      },
      error: error => this.handleError(error),
    });
  }

  accountLabel(account: AdminSystemAccount): string {
    return account.accountType === 'PlatformTreasury'
      ? 'Skarbiec platformy'
      : `${account.rateSourceCode ?? account.username} - ${account.rateSourceName ?? 'źródło kursów'}`;
  }

  accountTypeLabel(account: AdminSystemAccount): string {
    return account.accountType === 'PlatformTreasury'
      ? 'PlatformTreasury'
      : 'Konto płynności źródła';
  }

  private handleError(error: HttpErrorResponse): void {
    this.loading = false;
    this.errorMessage = error.status === 401 || error.status === 403
      ? 'Brak uprawnień administratora.'
      : 'Nie udało się pobrać sald kont systemowych.';
    this.changeDetector.markForCheck();
  }
}
