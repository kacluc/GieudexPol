import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router';

interface AuthResponse {
  token: string;
  email: string;
  userId: number; // Add userId to AuthResponse
}

interface User {
  id: number;
  email: string;
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
    const token = localStorage.getItem('authToken');
    const email = localStorage.getItem('userEmail');
    const userId = localStorage.getItem('userId');

    if (token && email && userId) {
      this.userSubject.next({ id: +userId, email });
    }
  }

  async register(email: string, password: string): Promise<void> {
    const response = await this.http.post<AuthResponse>(`${this.apiUrl}/register`, { email, password, confirmPassword: password }).toPromise();
    if (response?.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('userEmail', response.email);
      localStorage.setItem('userId', response.userId.toString()); // Store userId
      this.userSubject.next({ id: response.userId, email: response.email });
    }
  }

  async login(email: string, password: string): Promise<void> {
    const response = await this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).toPromise();
    if (response?.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('userEmail', response.email);
      localStorage.setItem('userId', response.userId.toString()); // Store userId
      this.userSubject.next({ id: response.userId, email: response.email });
      this.router.navigate(['/dashboard']);
    }
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userId'); // Remove userId
    this.userSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
