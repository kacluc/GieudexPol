# Model domenowy

## Główne agregaty

- użytkownik: `User`, `Wallet`, `Transaction`;
- kursy: `Currency`, `RateSource`, `ExchangeRate`;
- szybka wymiana: `ExchangeExecution`;
- rynek: `TradingPair`, `Order`, `TradeExecution`;
- alerty: `UserAlert`, `UserTradingAlert`, `AlertLog`, `UserAlertEvaluationState`, `Notification`;
- opłaty: `TransactionFee` i konto użytkownika typu `PlatformTreasury`.

## Kluczowe rozróżnienia

- `Role` jest claimem bezpieczeństwa.
- `AccountType` określa charakter biznesowy konta.
- `Balance` to saldo księgowe, `ReservedBalance` blokuje środki dla zleceń, a `AvailableBalance = Balance - ReservedBalance`.
- `ExchangeExecution` dokumentuje szybką wymianę ze źródłem.
- `TradeExecution` dokumentuje dopasowanie dwóch zleceń.

Szczegółowe pola i relacje opisują pliki `data-model.md` poszczególnych modułów.
