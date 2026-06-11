import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule]
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      displayName: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(50),
        Validators.pattern(/\S/)
      ]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordsMatch });
  }

  private passwordsMatch(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);

    if (this.registerForm.invalid || this.isSubmitting()) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    try {
      const { displayName, email, password } = this.registerForm.getRawValue();
      await this.authService.register(displayName.trim(), email.trim(), password);
      await this.router.navigate(['/auth/login']);
    } catch (error: unknown) {
      if (error instanceof HttpErrorResponse) {
        this.errorMessage.set(
          error.error?.detail ?? error.error?.message ?? 'Nie udało się utworzyć konta.'
        );
      } else {
        this.errorMessage.set('Błąd rejestracji. Spróbuj ponownie.');
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
