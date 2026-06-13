# API administratora

| Metoda | Trasa | Funkcja |
|---|---|---|
| GET/POST | `/api/admin/users` | Lista i tworzenie użytkownika |
| GET | `/api/admin/users/{id}` | Szczegóły |
| PUT | `/api/admin/users/{id}/role` | Zmiana roli |
| PUT | `/api/admin/users/{id}/reset-password` | Reset hasła |
| GET/POST | `/api/admin/test-exchange-rates` | Lista i tworzenie kursów mock |
| GET/PUT/DELETE | `/api/admin/test-exchange-rates/{id}` | Zarządzanie rekordem |
| POST | `/api/admin/alerts/evaluate` | Ręczna ewaluacja |
| GET | `/api/admin/system-accounts` | Konta i portfele systemowe |
| POST | `/api/admin/rate-source-orders` | Zlecenie konta źródła |

Wszystkie wymagają JWT z rolą `Admin`.

Przykład ręcznej ewaluacji:

```json
{
  "currencyCode": "EUR",
  "rateSourceCode": "MOCK_BANK_B"
}
```

Puste body ocenia wszystkie aktywne i spełnione alerty kursowe.
