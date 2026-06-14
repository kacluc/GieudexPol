import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Transaction } from '../../../../models/transaction.model';
import { TransactionService } from '../../../../services/transaction.service';
import { PaginatedResult } from '../../../../models/paginated-result.model';
import { FormGroup } from '@angular/forms';
import { Currency } from '../../../../models/currency.model';
import { WalletDto } from '../../../wallet/models/wallet-models';
import { FormBuilder } from '@angular/forms';
import { CeilPipe } from "../../../../transaction-transfer/transaction-transfer.component";

@Component({
  selector: 'app-transaction-history',
  standalone: true,
  imports: [CommonModule, RouterLink, CeilPipe],
  templateUrl: './transaction-history.component.html',
  styleUrl: './transaction-history.component.scss',
})
export class TransactionHistoryComponent implements OnInit {
  isLoading = true;
  transactionHistoryForm: FormGroup;
  currencies: Currency[] = [];
  userWallets: WalletDto[] = [];
  successMessage: string = '';
  currentUserId: number | null = null;
  transactions: Transaction[] = [];
  paginatedResult: PaginatedResult<Transaction> = { items: [], totalCount: 0, pageNumber: 1, pageSize: 10 };
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private readonly transactionService: TransactionService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {
        this.transactionHistoryForm = this.fb.group({
      transactionType: [null],
      currencyId: [null],
      startDate: [null],
      endDate: [null]
    })
}

  ngOnInit(): void {
    void this.loadTransactions();
  }

  async loadTransactions(): Promise<void> {
    const userId = Number(localStorage.getItem('userId'));

    if (!Number.isInteger(userId) || userId <= 0) {
      this.errorMessage = 'Brak identyfikatora zalogowanego użytkownika.';
      this.isLoading = false;
      return;
    }

    try {
      const result = await firstValueFrom(this.transactionService.getUserTransactions(userId));
      this.transactions = result.items;
    } catch (error) {
      console.error('Nie udało się załadować historii transakcji:', error);
      this.errorMessage = 'Nie można pobrać historii transakcji z API.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  transactionTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      OrderBookBuy: 'Kupno na rynku walut',
      OrderBookSell: 'Sprzedaż na rynku walut',
    };

    return labels[type] ?? type;
  }

  executionDetails(transaction: Transaction): string {
    if (!transaction.tradeExecutionId || !transaction.tradingPair) {
      if (transaction.exchangeExecutionId && transaction.exchangePair) {
        return `#${transaction.exchangeExecutionId}, ${transaction.exchangePair}, kurs ${
          transaction.exchangeRate?.toFixed(4) ?? '-'
        }, źródło ${transaction.rateSource ?? '-'}`;
      }

      return '-';
    }

    return `#${transaction.tradeExecutionId}, ${transaction.tradingPair} po ${
      transaction.executionPrice?.toFixed(4) ?? '-'
    }`;
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
}
