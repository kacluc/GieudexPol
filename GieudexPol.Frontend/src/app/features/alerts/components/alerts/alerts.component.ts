import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserAlertService } from '../../services/user-alert.service';
import { AlertType, UserAlertCreateDto, UserAlertUpdateDto, UserAlertDto } from '../../../../shared/models/user-alert.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyService } from '../../../../services/currency.service';
import { Currency } from '../../../../models/currency.model';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.css']
})
export class AlertsComponent implements OnInit {
  AlertType = AlertType; // Expose AlertType enum to the template
  alerts: UserAlertDto[] = [];
  alertForm: FormGroup;
  alertTypes = Object.values(AlertType);
  currencies: Currency[] = [];
  currentUserId: number | null = null;
  editingAlert: UserAlertDto | null = null;

  constructor(
    private userAlertService: UserAlertService,
    private fb: FormBuilder,
    private currencyService: CurrencyService,
    private authService: AuthService
  ) {
    this.alertForm = this.fb.group({
      currencyId: ['', Validators.required],
      alertType: ['', Validators.required],
      thresholdValue: [null],
      percentageChange: [null],
      timeFrameHours: [null],
      isActive: [true, Validators.required]
    });
  }

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.currentUserId = user ? user.id : null;
      // this.currentUserId = userId; // This line is no longer needed
      if (this.currentUserId) {
        this.loadAlerts();
        this.loadCurrencies();
      }
    });

    this.alertForm.get('alertType')?.valueChanges.subscribe(type => {
      this.updateValidators(type);
    });
  }

  loadAlerts(): void {
    if (this.currentUserId) {
      this.userAlertService.getUserAlerts(this.currentUserId).subscribe(alerts => {
        this.alerts = alerts;
      });
    }
  }

  loadCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe(currencies => {
      this.currencies = currencies;
    });
  }

  updateValidators(alertType: AlertType): void {
    const thresholdValueControl = this.alertForm.get('thresholdValue');
    const percentageChangeControl = this.alertForm.get('percentageChange');
    const timeFrameHoursControl = this.alertForm.get('timeFrameHours');

    thresholdValueControl?.clearValidators();
    percentageChangeControl?.clearValidators();
    timeFrameHoursControl?.clearValidators();

    switch (alertType) {
      case AlertType.Threshold:
        thresholdValueControl?.setValidators([Validators.required, Validators.min(0)]);
        break;
      case AlertType.PriceDrop:
      case AlertType.PriceIncrease:
        percentageChangeControl?.setValidators([Validators.required, Validators.min(0.01), Validators.max(100)]);
        timeFrameHoursControl?.setValidators([Validators.required, Validators.min(1)]);
        break;
      case AlertType.Volume:
        // Volume alert might require different fields or no specific validators here for now
        break;
    }

    thresholdValueControl?.updateValueAndValidity();
    percentageChangeControl?.updateValueAndValidity();
    timeFrameHoursControl?.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.alertForm.valid && this.currentUserId) {
      if (this.editingAlert) {
        // Update existing alert
        const updateDto: UserAlertUpdateDto = {
          id: this.editingAlert.id,
          currencyId: this.alertForm.value.currencyId,
          alertType: this.alertForm.value.alertType,
          thresholdValue: this.alertForm.value.thresholdValue,
          percentageChange: this.alertForm.value.percentageChange,
          timeFrameHours: this.alertForm.value.timeFrameHours,
          isActive: this.alertForm.value.isActive
        };
        this.userAlertService.updateUserAlert(updateDto.id, updateDto).subscribe(() => {
          this.loadAlerts();
          this.resetForm();
        });
      } else {
        // Create new alert
        const createDto: UserAlertCreateDto = {
          userId: this.currentUserId,
          currencyId: this.alertForm.value.currencyId,
          alertType: this.alertForm.value.alertType,
          thresholdValue: this.alertForm.value.thresholdValue,
          percentageChange: this.alertForm.value.percentageChange,
          timeFrameHours: this.alertForm.value.timeFrameHours
        };
        this.userAlertService.createUserAlert(createDto).subscribe(() => {
          this.loadAlerts();
          this.resetForm();
        });
      }
    }
  }

  editAlert(alert: UserAlertDto): void {
    this.editingAlert = alert;
    // The UserAlertDto does not have currencyId directly, it has currencySymbol.
    // We need to find the currencyId based on the currencySymbol.
    const selectedCurrency = this.currencies.find(c => c.symbol === alert.currencySymbol);
    this.alertForm.patchValue({
      currencyId: selectedCurrency ? selectedCurrency.id : 
      null,
      alertType: alert.alertType,
      thresholdValue: alert.thresholdValue,
      percentageChange: alert.percentageChange,
      timeFrameHours: alert.timeFrameHours,
      isActive: alert.isActive
    });
    this.updateValidators(alert.alertType);
  }

  deleteAlert(id: number): void {
    if (confirm('Are you sure you want to delete this alert?')) {
      this.userAlertService.deleteUserAlert(id).subscribe(() => {
        this.loadAlerts();
      });
    }
  }

  resetForm(): void {
    this.editingAlert = null;
    this.alertForm.reset({
      isActive: true
    });
    this.alertForm.get('alertType')?.clearValidators(); // Clear validators when resetting
    this.alertForm.get('thresholdValue')?.clearValidators();
    this.alertForm.get('percentageChange')?.clearValidators();
    this.alertForm.get('timeFrameHours')?.clearValidators();
    this.alertForm.get('alertType')?.updateValueAndValidity();
    this.alertForm.get('thresholdValue')?.updateValueAndValidity();
    this.alertForm.get('percentageChange')?.updateValueAndValidity();
    this.alertForm.get('timeFrameHours')?.updateValueAndValidity();


  }
}
