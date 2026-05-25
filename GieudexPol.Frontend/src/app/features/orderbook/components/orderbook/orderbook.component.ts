import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-orderbook',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './orderbook.component.html',
  styleUrl: './orderbook.component.scss',
})
export class OrderbookComponent {}
