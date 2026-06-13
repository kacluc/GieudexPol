# API rynku walut

Wymagany JWT.

| Metoda | Trasa | Opis |
|---|---|---|
| GET | `/api/trading-pairs` | Aktywne pary |
| GET | `/api/order-book?baseCurrencyCode=EUR&quoteCurrencyCode=PLN&depth=10` | Arkusz zagregowany |
| POST | `/api/orders` | Złożenie zlecenia użytkownika |
| GET | `/api/orders/my` | Własne zlecenia |
| DELETE | `/api/orders/{id}/cancel` | Anulowanie własnego zlecenia |
| POST | `/api/admin/rate-source-orders` | Zlecenie konta źródła; Admin |

```json
{
  "baseCurrencyCode": "EUR",
  "quoteCurrencyCode": "PLN",
  "side": "Buy",
  "price": 4.3000,
  "amount": 50.0000
}
```

Arkusz zwraca `buyOrders` malejąco oraz `sellOrders` rosnąco. Poziom ma `price`, sumę `amount`, narastające `total` i `ordersCount`.

Typowe błędy: `400` walidacja, `404` brak aktywnej pary, `409` brak środków lub niedozwolony stan anulowania.
