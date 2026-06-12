import { ChangeDetectorRef, Component, OnInit, Pipe, PipeTransform } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TransactionService } from '../services/transaction.service';
import { WalletService } from '../services/wallet.service';
import { CurrencyService } from '../services/currency.service';
import { AuthService } from '../features/auth/services/auth.service';
import { Currency } from '../models/currency.model';
import { Wallet } from '../models/wallet.model';
import { WalletDto } from '../models/wallet.dto';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { Transaction } from '../models/transaction.model';
import { PaginatedResult } from '../models/paginated-result.model';

@Pipe({ name: 'ceil', standalone: true })
export class CeilPipe implements PipeTransform {
  transform(value: number): number {
    return Math.ceil(value);
  }
}

@Component({
  selector: 'app-transaction-transfer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CeilPipe],
  templateUrl: './transaction-transfer.component.html',
  styleUrls: ['./transaction-transfer.component.css']
})
export class TransactionTransferComponent implements OnInit {
  transferForm: FormGroup;
  transactionHistoryForm: FormGroup;
  currencies: Currency[] = [];
  userWallets: WalletDto[] = [];
  errorMessage: string = '';
  successMessage: string = '';
  currentUserId: number | null = null;
  transactions: Transaction[] = [];
  paginatedResult: PaginatedResult<Transaction> = { items: [], totalCount: 0, pageNumber: 1, pageSize: 10 };
  transactionTypes: string[] = ['Transfer', 'Deposit', 'Withdrawal', 'Buy', 'Sell'];

  get transferableCurrencies(): Currency[] {
    const walletCurrencyIds = new Set(this.userWallets.map(wallet => wallet.currencyId));
    return this.currencies.filter(currency => walletCurrencyIds.has(currency.id));
  }

  constructor(
    private fb: FormBuilder,
    private transactionService: TransactionService,
    private walletService: WalletService,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private router: Router,
    private changeDetector: ChangeDetectorRef
  ) {
    this.transferForm = this.fb.group({
      receiverUsername: ['', [Validators.required, Validators.email]],
      amount: ['', [Validators.required, Validators.min(0.01)]],
      currencyId: ['', Validators.required]
    });

    this.transactionHistoryForm = this.fb.group({
      transactionType: [null],
      currencyId: [null],
      startDate: [null],
      endDate: [null]
    });
  }

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      if (user && user.id) {
        this.currentUserId = user.id;
        this.loadUserWallets(this.currentUserId);
        this.loadUserTransactions();
      }
    });
    this.loadCurrencies();

    this.transactionHistoryForm.valueChanges.subscribe(() => {
      this.loadUserTransactions();
    });
  }

  loadUserWallets(userId: number): void {
    this.walletService.getUserWallets(userId).subscribe(
      (wallets) => {
        this.userWallets = wallets;
        this.changeDetector.detectChanges();
      },
      (error) => {
        console.error('Error loading user wallets:', error);
        this.errorMessage = 'Failed to load user wallets.';
        this.changeDetector.detectChanges();
      }
    );
  }

  loadCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe(
      (currencies) => {
        this.currencies = currencies;
        this.changeDetector.detectChanges();
      },
      (error) => {
        console.error('Error loading currencies:', error);
        this.errorMessage = 'Failed to load currencies.';
        this.changeDetector.detectChanges();
      }
    );
  }

  loadUserTransactions(pageNumber: number = this.paginatedResult.pageNumber, pageSize: number = this.paginatedResult.pageSize): void {
    if (this.currentUserId) {
      const filters = this.transactionHistoryForm.value;
      this.transactionService.getUserTransactions(
        this.currentUserId,
        pageNumber,
        pageSize,
        filters.transactionType,
        filters.currencyId,
        filters.startDate,
        filters.endDate
      ).subscribe(
        (result) => {
          this.paginatedResult = result;
          this.transactions = result.items;
          this.changeDetector.detectChanges();
        },
        (error) => {
          console.error('Error loading transactions:', error);
          this.errorMessage = 'Failed to load transaction history.';
          this.changeDetector.detectChanges();
        }
      );
    }
  }

  onPageChange(pageNumber: number): void {
    this.loadUserTransactions(pageNumber, this.paginatedResult.pageSize);
  }

  transactionTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      OrderBookBuy: 'Kupno w arkuszu zleceń',
      OrderBookSell: 'Sprzedaż w arkuszu zleceń',
    };

    return labels[type] ?? type;
  }

  onSubmit(): void {
    if (this.transferForm.valid && this.currentUserId) {
      this.errorMessage = '';
      this.successMessage = '';

      const { receiverUsername, amount, currencyId } = this.transferForm.value;
      const request = {
        receiverUsername: receiverUsername.trim(),
        amount: amount,
        currencyId: currencyId
      };

      this.transactionService.createTransfer(request).subscribe(
        (response) => {
          this.successMessage = 'Transfer zakończony pomyślnie.';
          this.transferForm.reset();
          if (this.currentUserId) {
            this.loadUserWallets(this.currentUserId);
            this.loadUserTransactions(); // Refresh transaction history after successful transfer
          }
          this.changeDetector.detectChanges();
        },
        (error) => {
          console.error('Transaction failed:', error);
          this.errorMessage = error.error?.message || 'Nie udało się wykonać transferu.';
          this.changeDetector.detectChanges();
        }
      );
    } else {
      this.errorMessage = 'Uzupełnij poprawnie wszystkie wymagane pola.';
    }
  }
}
