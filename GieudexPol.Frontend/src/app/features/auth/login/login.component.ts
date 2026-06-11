import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);

    if (this.loginForm.invalid || this.isSubmitting()) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    try {
      const { email, password } = this.loginForm.getRawValue();
      await this.authService.login(email, password);
    } catch (error: unknown) {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.errorMessage.set(
          error.error?.detail ?? 'Nieprawidłowy adres e-mail lub hasło.'
        );
      } else {
        this.errorMessage.set('Błąd logowania. Spróbuj ponownie.');
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
