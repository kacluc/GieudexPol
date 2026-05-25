import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

interface FavoriteCurrencyDto {
    currencyCode: string;
}

@Injectable({
    providedIn: 'root'
})
export class FavoriteCurrencyService {

    private readonly apiUrl = '/api/favorites';

    constructor(private http: HttpClient) { }

    getFavorites(): Observable<string[]> {
        return this.http.get<FavoriteCurrencyDto[]>(this.apiUrl).pipe(
            map(items => items.map(item => item.currencyCode))
        );
    }

    addFavorite(currencyCode: string): Observable<void> {
        return this.http.post<void>(this.apiUrl, { currencyCode });
    }

    removeFavorite(currencyCode: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${currencyCode}`);
    }
}
