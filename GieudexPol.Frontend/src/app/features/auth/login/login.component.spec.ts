import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { AuthService } from '../services/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;

  const authService = {
    login: vi.fn(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders an invalid credentials error immediately after a 401 response', async () => {
    authService.login.mockRejectedValueOnce(new HttpErrorResponse({
      status: 401,
      error: { detail: 'Nieprawidłowy adres e-mail lub hasło.' },
    }));
    component.loginForm.setValue({
      email: 'user@example.com',
      password: 'wrong-password',
    });

    await component.onSubmit();
    await fixture.whenStable();

    const error = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement | null;
    expect(error?.textContent).toContain('Nieprawidłowy adres e-mail lub hasło.');
    expect(component.isSubmitting()).toBe(false);
  });
});
