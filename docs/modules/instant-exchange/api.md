# API szybkiej wymiany

## Wykonanie

`POST /api/Wallets/trade?userId={id}`; JWT i zgodność właściciela.

```json
{
  "fromCurrencyId": 1,
  "amountFrom": 3900.00,
  "toCurrencyId": 3
}
```

Odpowiedź zawiera m.in. `amountTo`, waluty, źródło, zastosowany kurs, fee, datę i `exchangeExecutionId`.

## Preview

`POST /api/wallet/exchange/preview`; JWT, bez `userId` w body.

```json
{
  "fromCurrencyId": 1,
  "toCurrencyId": 3,
  "amount": 3900.00
}
```

```json
{
  "fromCurrencyCode": "PLN",
  "toCurrencyCode": "USD",
  "inputAmount": 3900.00,
  "estimatedOutputAmount": 1000.00,
  "rate": 0.2564,
  "feeAmount": 19.50,
  "feeCurrencyCode": "PLN",
  "totalDebitAmount": 3919.50,
  "rateDate": "2026-06-13",
  "hasSufficientFunds": true,
  "isPreview": true
}
```

`400` oznacza m.in. brak kursu, brak płynności, równą parę lub niedodatnią kwotę.
