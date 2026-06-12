import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { OrderBookService } from './features/orderbook/services/order-book.service';
import { WalletService } from './features/wallet/services/wallet.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  constructor(
    walletService: WalletService,
    orderBookService: OrderBookService,
  ) {
    walletService.initialize();
    orderBookService.initialize();
  }
}
