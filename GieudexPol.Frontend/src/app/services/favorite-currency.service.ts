import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class FavoriteCurrencyService {

    private apiUrl = 'http://localhost:5265/api/favorites';

    constructor(private http: HttpClient) { }

    getFavorites(): Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl);
    }

    addFavorite(currencyCode: string): Observable<any> {
        return this.http.post(this.apiUrl, {
            currencyCode: currencyCode
        });
    }

    removeFavorite(currencyCode: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${currencyCode}`);
    }
}