# API portfeli

Wymagany JWT.

| Metoda | Trasa | Opis |
|---|---|---|
| GET | `/api/Wallets/user/{userId}` | Portfele bieżącego użytkownika |
| GET | `/api/Wallets/available-currencies?userId=` | Waluty możliwe do dodania |
| GET | `/api/Wallets/{id}` | Portfel właściciela |
| POST | `/api/Wallets/user/{userId}/currencies/{currencyId}` | Dodanie portfela |
| POST | `/api/Wallets/deposit?userId=` | Wpłata |
| POST | `/api/Wallets/withdraw?userId=` | Wypłata |
| POST | `/api/Transactions/transfer` | Transfer |
| GET | `/api/Transactions/user/{userId}` | Historia z filtrami i paginacją |

Wpłata/wypłata:

```json
{ "currencyId": 1, "amount": 1000.00 }
```

Transfer:

```json
{
  "receiverUsername": "janusz.kowalski@gieudexpol.local",
  "currencyId": 1,
  "amount": 100.00
}
```

Typowe odpowiedzi: `401` brak JWT, `403` cudzy `userId`, `400` walidacja, `409` brak środków przy transferze.
