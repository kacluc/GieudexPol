import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });

    // Dodanie walidacji pasma haseł po inicjalizacji formularza
    this.registerForm.get('confirmPassword')?.setValidators([
        Validators.required,
        this.matchPasswords.bind(this)
    ]);
  }

  matchPasswords(control: any): { [key: string]: any } | null {
    if (!this.registerForm) {
      return null; // Form not initialized yet
    }
    const password = this.registerForm.get('password')?.value;
    const confirmPassword = control.value;
    return password === confirmPassword ? null : { 'passwordMismatch': true };
  }

  async onSubmit(): Promise<void> {
    this.errorMessage = null;

    // Zabezpieczenie przed undefined – informuje TS, że formularz istnieje
    if (!this.registerForm) {
      return;
    }

    if (this.registerForm.valid) {
      try {
        const { email, password } = this.registerForm.value;
        await this.authService.register(email, password);
        this.router.navigate(['/auth/login']);
      } catch (error: any) {
        this.errorMessage = error.message || 'Błąd rejestracji. Spróbuj ponownie.';
      }
    }
  }
}
