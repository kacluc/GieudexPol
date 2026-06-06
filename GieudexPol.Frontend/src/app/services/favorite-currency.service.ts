import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

interface FavoriteCurrencyDto {
    currencyCode: string;
}

@Injectable({
    providedIn: 'root'
})
export class FavoriteCurrencyService {
    private readonly apiUrl = '/api/favorites';
    private favoritesSubject$: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);

    favorites$ = this.favoritesSubject$.asObservable().pipe(shareReplay(1));

    constructor(private http: HttpClient) {
        this.loadFavorites();
    }

    private loadFavorites(): void {
        this.getFavorites().subscribe({
            next: (data) => {
                this.favoritesSubject$.next(data);
            },
            error: (error) => {
                console.error('Nie udalo sie pobrac ulubionych walut.', error);
            }
        });
    }

    getFavorites(): Observable<string[]> {
        return this.http.get<FavoriteCurrencyDto[]>(this.apiUrl).pipe(
            map(items => items.map(item => item.currencyCode))
        );
    }

    addFavorite(currencyCode: string): Observable<void> {
        return this.http.post<void>(this.apiUrl, { currencyCode }).pipe(
            map(() => {
                this.favoritesSubject$.next([...this.favoritesSubject$.value, currencyCode]);
                return;
            })
        );
    }

    removeFavorite(currencyCode: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${currencyCode}`).pipe(
            map(() => {
                this.favoritesSubject$.next(this.favoritesSubject$.value.filter(item => item !== currencyCode));
                return;
            })
        );
    }
}