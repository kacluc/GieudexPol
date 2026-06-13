# API prowizji i skarbca

Nie ma publicznego endpointu do samodzielnego liczenia ani konfigurowania prowizji. Kalkulator jest wywoływany przez endpointy operacji:

- `/api/Wallets/deposit`;
- `/api/Wallets/withdraw`;
- `/api/Transactions/transfer`;
- `/api/Wallets/trade`;
- `/api/wallet/exchange/preview`;
- `/api/orders`.

Podgląd portfeli skarbca:

```http
GET /api/admin/system-accounts
Authorization: Bearer {admin-token}
```

Odpowiedź zawiera konto typu `PlatformTreasury` wraz z `balance`, `reservedBalance` i `availableBalance` dla każdej waluty.

## Ograniczenie

Nie ma endpointu zmiany 0,5% ani minimum 10 PLN. Panel konfiguracji prowizji nie jest zaimplementowany.
