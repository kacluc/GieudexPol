# Prowizje i PlatformTreasury

## Cel

Centralny kalkulator nalicza prowizje operacji, a konto `PlatformTreasury` gromadzi je w portfelach odpowiednich walut.

## Zakres

Prowizja jest stosowana do:

- wpłat;
- wypłat;
- transferów;
- szybkiej wymiany;
- obu stron wykonania zlecenia.

## Serwisy i encje

`TransactionFeeCalculator`, `TransactionFee`, `WalletRepository`, `SystemAccountService`, `User` typu `PlatformTreasury`.

`TransactionFee` przechowuje aktywną definicję operacji i jej identyfikator dla historii. Sam kalkulator używa stałej reguły 0,5% i minimum 10 PLN.

## Co pokazać

Porównać operację 100 PLN i 10 000 PLN, a następnie pokazać wzrost portfela PLN skarbca.
