import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { UserAlertService } from '../../features/alerts/services/user-alert.service';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements OnInit, OnDestroy {
  readonly userEmail = localStorage.getItem('userEmail') ?? '';
  readonly navItems = [
    { label: 'Dashboard', path: '/' },
    { label: 'Kursy walut', path: '/rates' },
    { label: 'Ulubione', path: '/converter' },
    { label: 'Portfel', path: '/wallet' },
    { label: 'Transfer', path: '/transfer' },
    { label: 'Historia', path: '/history' },
    { label: 'Arkusz zleceń', path: '/order-book' },
    { label: 'Alerty', path: '/alerts' },
    { label: 'Top Walenie', path: '/whale-ranking' },
  ];

  private refreshTimer: number | null = null;

  constructor(
    private readonly authService: AuthService,
    readonly userAlertService: UserAlertService,
  ) {}

  ngOnInit(): void {
    this.refreshAlertIndicator();
    this.refreshTimer = window.setInterval(
      () => this.refreshAlertIndicator(),
      60_000,
    );
  }

  ngOnDestroy(): void {
    if (this.refreshTimer != null) {
      window.clearInterval(this.refreshTimer);
    }
  }

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  logout(): void {
    this.authService.logout();
  }

  private refreshAlertIndicator(): void {
    this.userAlertService.getMyAlerts().subscribe({
      error: () => this.userAlertService.hasUnacknowledgedAlerts.set(false),
    });
  }
}
