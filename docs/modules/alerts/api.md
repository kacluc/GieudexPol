# API alertów

Wymagany JWT.

## Kursowe

| Metoda | Trasa |
|---|---|
| GET | `/api/UserAlerts/me` |
| GET | `/api/UserAlerts/user/{userId}` |
| GET | `/api/UserAlerts/rate-sources` |
| POST | `/api/UserAlerts` |
| PUT | `/api/UserAlerts/{id}` |
| DELETE | `/api/UserAlerts/{id}` |

## Rynkowe

| Metoda | Trasa |
|---|---|
| GET | `/api/trading-alerts/me` |
| POST | `/api/trading-alerts` |
| PUT | `/api/trading-alerts/{id}` |
| DELETE | `/api/trading-alerts/{id}` |

Klient nie może sam ustawić `Fulfilled`; stan ten nadaje ewaluator. Może przełączyć alert na `Inactive` lub ponownie `Active`.

## Powiadomienia

- `GET /api/Notifications/me`
- `PUT /api/Notifications/{id}/mark-as-read`

Odczyt powiadomienia sprawdza właściciela.

## Admin

`POST /api/admin/alerts/evaluate` przyjmuje opcjonalnie `alertId`, `currencyCode`, `rateSourceCode`.
