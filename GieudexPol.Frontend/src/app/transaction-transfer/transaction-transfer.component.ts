import { Component, OnInit } from '@angular/core';
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

@Component({
  selector: 'app-transaction-transfer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './transaction-transfer.component.html',
  styleUrls: ['./transaction-transfer.component.css']
})
export class TransactionTransferComponent implements OnInit {
  transferForm: FormGroup;
  currencies: Currency[] = [];
  userWallets: WalletDto[] = [];
  errorMessage: string = '';
  successMessage: string = '';
  currentUserId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private transactionService: TransactionService,
    private walletService: WalletService,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private router: Router
  ) {
    this.transferForm = this.fb.group({
      receiverUsername: ['', Validators.required],
      amount: ['', [Validators.required, Validators.min(0.01)]],
      currencyId: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      if (user && user.id) {
        this.currentUserId = user.id;
        this.loadUserWallets(this.currentUserId);
      }
    });
    this.loadCurrencies();
  }

  loadUserWallets(userId: number): void {
    this.walletService.getUserWallets(userId).subscribe(
      (wallets) => {
        this.userWallets = wallets;
      },
      (error) => {
        console.error('Error loading user wallets:', error);
        this.errorMessage = 'Failed to load user wallets.';
      }
    );
  }

  loadCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe(
      (currencies) => {
        this.currencies = currencies;
      },
      (error) => {
        console.error('Error loading currencies:', error);
        this.errorMessage = 'Failed to load currencies.';
      }
    );
  }

  onSubmit(): void {
    if (this.transferForm.valid && this.currentUserId) {
      this.errorMessage = '';
      this.successMessage = '';

      const { receiverUsername, amount, currencyId } = this.transferForm.value;
      const request = {
        senderId: this.currentUserId,
        receiverUsername: receiverUsername,
        amount: amount,
        currencyId: currencyId
      };

      this.transactionService.createTransfer(request).subscribe(
        (response) => {
          this.successMessage = 'Transaction successful!';
          this.transferForm.reset();
          // Optionally, refresh wallets or navigate
          if (this.currentUserId) {
            this.loadUserWallets(this.currentUserId);
          }
        },
        (error) => {
          console.error('Transaction failed:', error);
          this.errorMessage = error.error?.message || 'Transaction failed. Please try again.';
        }
      );
    } else {
      this.errorMessage = 'Please fill all required fields correctly.';
    }
  }
}
