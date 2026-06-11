import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

interface AuthResponse {
  token: string;
  email: string;
  userId: number;
  role: UserRole;
}

export type UserRole = 'Admin' | 'User';

export interface AuthenticatedUser {
  id: number;
  email: string;
  role: UserRole;
}

interface JwtPayload {
  exp?: number;
  role?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/Auth';
  private userSubject = new BehaviorSubject<AuthenticatedUser | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.loadUserFromLocalStorage();
  }

  private loadUserFromLocalStorage(): void {
    const token = this.getToken();
    const email = localStorage.getItem('userEmail');
    const userId = localStorage.getItem('userId');
    const role = token ? this.getRoleFromToken(token) : null;

    if (token && email && userId && role && Number.isInteger(+userId) && +userId > 0) {
      localStorage.setItem('userRole', role);
      this.userSubject.next({ id: Number(userId), email, role });
      return;
    }

    this.clearSession();
  }

  async register(displayName: string, email: string, password: string): Promise<void> {
    const response = await this.http.post<AuthResponse>(`${this.apiUrl}/register`, {
      displayName,
      email,
      password,
      confirmPassword: password
    }).toPromise();
    if (response?.token) {
      this.persistSession(response);
    }
  }

  async login(email: string, password: string): Promise<void> {
    const response = await this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).toPromise();
    if (response?.token) {
      this.persistSession(response);
      this.router.navigate(['/']);
    }
  }

  logout(): void {
    this.clearSession();
    this.router.navigate(['/auth/login']);
  }

  private clearSession(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userId');
    localStorage.removeItem('userRole');
    this.userSubject.next(null);
  }

  getToken(): string | null {
    const token = localStorage.getItem('authToken');

    if (!token || !this.hasValidExpiration(token)) {
      this.clearSession();
      return null;
    }

    return token;
  }

  isAuthenticated(): boolean {
    return this.getToken() !== null;
  }

  isAdmin(): boolean {
    return this.userSubject.value?.role === 'Admin';
  }

  private persistSession(response: AuthResponse): void {
    const role = this.getRoleFromToken(response.token) ?? response.role;
    localStorage.setItem('authToken', response.token);
    localStorage.setItem('userEmail', response.email);
    localStorage.setItem('userId', response.userId.toString());
    localStorage.setItem('userRole', role);
    this.userSubject.next({
      id: response.userId,
      email: response.email,
      role,
    });
  }

  private getRoleFromToken(token: string): UserRole | null {
    const payload = this.decodeToken(token);
    const role = payload?.role ??
      payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    return role === 'Admin' || role === 'User' ? role : null;
  }

  private hasValidExpiration(token: string): boolean {
    const payload = this.decodeToken(token);
    return typeof payload?.exp === 'number' && payload.exp * 1000 > Date.now();
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      const payloadPart = token.split('.')[1];
      if (!payloadPart) {
        return null;
      }

      const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
      const paddedBase64 = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
      return JSON.parse(atob(paddedBase64)) as JwtPayload;
    } catch {
      return null;
    }
  }
}
