import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Transaction } from '../../../../models/transaction.model';
import { TransactionService } from '../../../../services/transaction.service';

@Component({
  selector: 'app-transaction-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './transaction-history.component.html',
  styleUrl: './transaction-history.component.scss',
})
export class TransactionHistoryComponent implements OnInit {
  transactions: Transaction[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private readonly transactionService: TransactionService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {}

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
      return '-';
    }

    return `#${transaction.tradeExecutionId}, ${transaction.tradingPair} po ${
      transaction.executionPrice?.toFixed(4) ?? '-'
    }`;
  }
}
