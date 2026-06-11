import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent {
  readonly userEmail = localStorage.getItem('userEmail') ?? '';
  readonly navItems = [
    { label: 'Dashboard', path: '/' },
    { label: 'Kursy walut', path: '/rates' },
    { label: 'Ulubione', path: '/converter' },
    { label: 'Portfel', path: '/wallet' },
    { label: 'Transfer', path: '/transfer' },
    { label: 'Historia', path: '/history' },
    { label: 'Order book', path: '/orderbook' },
    { label: 'Alerty', path: '/alerts' },
    { label: 'Top Walenie', path: '/whale-ranking' },
  ];

  constructor(private readonly authService: AuthService) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  logout(): void {
    this.authService.logout();
  }
}
