import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { AuthService } from '../services/auth.service';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let fixture: ComponentFixture<RegisterComponent>;
  let component: RegisterComponent;
  const authService = { register: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders the styled registration form with all controls', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.auth-container form')).toBeTruthy();
    expect(element.querySelectorAll('input')).toHaveLength(4);
    expect(component.registerForm.get('displayName')).toBeTruthy();
    expect(component.registerForm.get('email')).toBeTruthy();
    expect(component.registerForm.get('password')).toBeTruthy();
    expect(component.registerForm.get('confirmPassword')).toBeTruthy();
  });

  it('rejects passwords that do not match', () => {
    component.registerForm.setValue({
      displayName: 'Test User',
      email: 'user@example.com',
      password: 'password',
      confirmPassword: 'different',
    });

    expect(component.registerForm.hasError('passwordMismatch')).toBe(true);
    expect(component.registerForm.valid).toBe(false);
  });

  it('sends the display name during registration', async () => {
    authService.register.mockResolvedValueOnce(undefined);
    component.registerForm.setValue({
      displayName: '  Test User  ',
      email: 'user@example.com',
      password: 'password',
      confirmPassword: 'password',
    });

    await component.onSubmit();

    expect(authService.register).toHaveBeenCalledWith(
      'Test User',
      'user@example.com',
      'password'
    );
  });
});
