# API kursów walut

Wszystkie trasy kontrolera wymagają JWT.

| Metoda | Trasa | Opis |
|---|---|---|
| GET | `/api/ExchangeRates/{base}/{target}` | Kurs pary |
| GET | `/api/ExchangeRates/chart` | Punkty wykresu |
| GET | `/api/ExchangeRates/latest` | Najnowsze kursy źródła |
| GET | `/api/ExchangeRates` | Wszystkie kursy |
| POST | `/api/ExchangeRates/sync/nbp` | Synchronizacja NBP |
| POST | `/api/ExchangeRates/sync/{sourceCode}` | Synchronizacja wskazanego źródła |
| POST/PUT/DELETE | `/api/ExchangeRates...` | Ogólny CRUD kursów |

Przykład:

```http
GET /api/ExchangeRates/chart?currency=EUR&source=ECB&from=2026-01-01&to=2026-06-13
Authorization: Bearer {token}
```

Panel admina:

| Metoda | Trasa | Dostęp |
|---|---|---|
| GET | `/api/admin/test-exchange-rates/sources` | Admin |
| GET | `/api/admin/test-exchange-rates` | Admin |
| POST | `/api/admin/test-exchange-rates` | Admin |
| PUT | `/api/admin/test-exchange-rates/{id}` | Admin |
| DELETE | `/api/admin/test-exchange-rates/{id}` | Admin |

Typowe błędy: `400` niepoprawny zakres lub dane, `403` próba modyfikacji chronionego źródła, `404` brak źródła/rekordu, `409` konflikt kursu dla dnia.
