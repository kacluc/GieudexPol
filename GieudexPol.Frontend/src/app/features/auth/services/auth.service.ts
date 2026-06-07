import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

interface AuthResponse {
  token: string;
  email: string;
  userId: number;
}

interface User {
  id: number;
  email: string;
}

interface JwtPayload {
  exp?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/Auth';
  private userSubject = new BehaviorSubject<User | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.loadUserFromLocalStorage();
  }

  private loadUserFromLocalStorage(): void {
    const token = this.getToken();
    const email = localStorage.getItem('userEmail');
    const userId = localStorage.getItem('userId');

    if (token && email && userId && Number.isInteger(+userId) && +userId > 0) {
      this.userSubject.next({ id: Number(userId), email });
      return;
    }

    this.clearSession();
  }

  async register(email: string, password: string): Promise<void> {
    const response = await this.http.post<AuthResponse>(`${this.apiUrl}/register`, { email, password, confirmPassword: password }).toPromise();
    if (response?.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('userEmail', response.email);
      localStorage.setItem('userId', response.userId.toString());
      this.userSubject.next({ id: response.userId, email: response.email });
    }
  }

  async login(email: string, password: string): Promise<void> {
    const response = await this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).toPromise();
    if (response?.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('userEmail', response.email);
      localStorage.setItem('userId', response.userId.toString());
      this.userSubject.next({ id: response.userId, email: response.email });
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

  private hasValidExpiration(token: string): boolean {
    try {
      const payloadPart = token.split('.')[1];
      if (!payloadPart) {
        return false;
      }

      const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
      const paddedBase64 = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
      const payload = JSON.parse(atob(paddedBase64)) as JwtPayload;

      return typeof payload.exp === 'number' && payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }
}
