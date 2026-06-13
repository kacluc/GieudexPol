# API kont systemowych

## Podgląd

`GET /api/admin/system-accounts`, rola `Admin`.

Zwraca:

- techniczną nazwę i nazwę wyświetlaną;
- `AccountType`;
- kod/nazwę i aktywność powiązanego źródła;
- portfele ze stanem całkowitym, zarezerwowanym i dostępnym.

## Zlecenie źródła

`POST /api/admin/rate-source-orders`, rola `Admin`.

```json
{
  "rateSourceCode": "ECB",
  "baseCurrencyCode": "EUR",
  "quoteCurrencyCode": "PLN",
  "side": "Sell",
  "price": 4.3000,
  "amount": 1000.0000
}
```

Nie ma endpointu logowania jako konto systemowe ani zwykłego endpointu transferu do niego.
