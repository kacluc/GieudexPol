# Model danych portfeli

## Wallet

- `Id`
- `UserId`
- `CurrencyId`
- `Balance`
- `ReservedBalance`
- wyliczane `AvailableBalance`

Metody domenowe: `Debit`, `DebitReserved`, `Reserve`, `Release`, `Credit`.

## Transaction

Przechowuje nadawcę, odbiorcę, walutę, kwotę, status, typ, zastosowaną prowizję, czas oraz opcjonalne powiązanie z `TransactionFee`, `TradeExecution` i `ExchangeExecution`.

## Indeksy i relacje

- unikalny indeks portfela po użytkowniku i walucie;
- transakcja ma dwa powiązania do `User`: nadawca i odbiorca;
- usunięcie wykonania nie powinno usuwać historii transakcji.
