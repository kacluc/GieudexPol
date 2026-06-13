# Portfele

## Cel

Portfel przechowuje saldo jednej waluty dla jednego konta. Moduł realizuje wpłaty, wypłaty, transfery, dodawanie walut i udostępnia historię operacji.

## Encje i serwisy

- `Wallet`, `Transaction`, `Currency`;
- `WalletService`, `TransactionService`;
- `WalletRepository`, `TransactionRepository`;
- `TransactionFeeCalculator`.

## Scenariusze

- wpłata: użytkownik otrzymuje kwotę pomniejszoną o prowizję;
- wypłata: saldo jest pomniejszane o kwotę i prowizję;
- transfer: odbiorca dostaje kwotę, nadawca płaci kwotę i prowizję;
- zlecenie: część salda przechodzi do `ReservedBalance`;
- historia: pokazuje zwykłe operacje, wykonania rynku i szybkiej wymiany.

Kontrolery pobierają tożsamość z JWT i blokują podmienienie `userId`.
