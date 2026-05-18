import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // Potrzebne dla *ngIf i *ngFor
import { RouterLinkActive, RouterLink } from '@angular/router'; // Dodano RouterLink do importów

@Component({
  selector: 'app-navbar',
  standalone: true, 
  imports: [CommonModule, RouterLinkActive, RouterLink], 
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  // Lista linków do kluczowych sekcji aplikacji
  public navItems = [
    { label: 'Dashboard', path: '' }, // Link do głównego widoku
    { label: 'Portfel', path: 'wallet' }, // Nowy moduł Portfela
    { label: 'Kursy walut', path: 'rates' },
    { label: 'Historia Transakcji', path: '/history' } // Założona trasa dla historii
  ];

  constructor() { }
}
