import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { UserAlert } from '../../models/alert.model';
import { AlertsService } from '../../services/alerts.service';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './alerts.component.html',
  styleUrl: './alerts.component.scss',
})
export class AlertsComponent implements OnInit {
  alerts: UserAlert[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private readonly alertsService: AlertsService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    void this.loadAlerts();
  }

  async loadAlerts(): Promise<void> {
    const userId = Number(localStorage.getItem('userId'));

    if (!Number.isInteger(userId) || userId <= 0) {
      this.errorMessage = 'Brak identyfikatora zalogowanego uzytkownika.';
      this.isLoading = false;
      return;
    }

    try {
      this.alerts = await firstValueFrom(this.alertsService.getUserAlerts(userId));
    } catch (error) {
      console.error('Nie udalo sie zaladowac alertow:', error);
      this.errorMessage = 'Nie mozna pobrac alertow cenowych z API.';
    } finally {
      this.isLoading = false;
      this.changeDetector.detectChanges();
    }
  }
}
