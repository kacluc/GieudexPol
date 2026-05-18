import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router';

interface AuthResponse {
  token: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/Auth';

  constructor(private http: HttpClient, private router: Router) { }

  async register(email: string, password: string): Promise<void> {
    const response = await this.http.post<AuthResponse>(`${this.apiUrl}/register`, { email, password, confirmPassword: password }).toPromise();
    if (response?.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('userEmail', response.email);
    }
  }

    async login(email: string, password: string): Promise<void> {
      const response = await this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).toPromise();
      if (response?.token) {
        localStorage.setItem('authToken', response.token);
        localStorage.setItem('userEmail', response.email);
        // PRZEKIEROWANIE DO DASHBOARDU PO PŁYWNEJ AUTORYZACJI
        this.router.navigate(['/dashboard']); 
      }
    }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userEmail');
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
