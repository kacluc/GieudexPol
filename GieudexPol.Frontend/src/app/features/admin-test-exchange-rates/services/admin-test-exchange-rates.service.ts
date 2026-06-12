import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AdminTestExchangeRate,
  AdminTestExchangeRateFilters,
  AdminTestRateSource,
  AlertEvaluationRequest,
  AlertEvaluationResult,
  CreateTestExchangeRate,
  UpdateTestExchangeRate,
} from '../models/admin-test-exchange-rate.model';

@Injectable({
  providedIn: 'root',
})
export class AdminTestExchangeRatesService {
  private readonly apiUrl = '/api/admin/test-exchange-rates';

  constructor(private readonly http: HttpClient) {}

  getTestRateSources(): Observable<AdminTestRateSource[]> {
    return this.http.get<AdminTestRateSource[]>(`${this.apiUrl}/sources`);
  }

  getTestExchangeRates(
    filters: AdminTestExchangeRateFilters = {},
  ): Observable<AdminTestExchangeRate[]> {
    let params = new HttpParams();

    if (filters.rateSourceCode) {
      params = params.set('rateSourceCode', filters.rateSourceCode);
    }
    if (filters.currencyId) {
      params = params.set('currencyId', filters.currencyId);
    }
    if (filters.currencyCode) {
      params = params.set('currencyCode', filters.currencyCode);
    }
    if (filters.dateFrom) {
      params = params.set('dateFrom', filters.dateFrom);
    }
    if (filters.dateTo) {
      params = params.set('dateTo', filters.dateTo);
    }

    return this.http.get<AdminTestExchangeRate[]>(this.apiUrl, { params });
  }

  getTestExchangeRate(id: number): Observable<AdminTestExchangeRate> {
    return this.http.get<AdminTestExchangeRate>(`${this.apiUrl}/${id}`);
  }

  createTestExchangeRate(
    request: CreateTestExchangeRate,
  ): Observable<AdminTestExchangeRate> {
    return this.http.post<AdminTestExchangeRate>(this.apiUrl, request);
  }

  updateTestExchangeRate(
    id: number,
    request: UpdateTestExchangeRate,
  ): Observable<AdminTestExchangeRate> {
    return this.http.put<AdminTestExchangeRate>(`${this.apiUrl}/${id}`, request);
  }

  deleteTestExchangeRate(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  evaluateAlerts(
    request: AlertEvaluationRequest = {},
  ): Observable<AlertEvaluationResult> {
    return this.http.post<AlertEvaluationResult>(
      '/api/admin/alerts/evaluate',
      request,
    );
  }
}
