# Model danych kursów

## Currency

`Id`, `Symbol`, `Name`, `IsActive`. Waluta ma portfele, kursy, transakcje, alerty i powiązania z parami handlowymi.

## RateSource

`Id`, unikalny `Code`, `Name`, `IsActive`, opcjonalny `SystemUserId`. Powiązanie z kontem systemowym umożliwia ocenę płynności i wykonanie szybkiej wymiany.

## ExchangeRate

`Id`, `CurrencyId`, `RateSourceId`, `BuyPrice`, `SellPrice`, opcjonalny `MidPrice`, `EffectiveDate`, `FetchedAt`.

Indeks unikalny obejmuje `(CurrencyId, RateSourceId, EffectiveDate)`.

## Uwagi

PLN jest walutą bazową katalogu i zwykle nie wymaga osobnego `ExchangeRate`. Kursy źródeł referencyjnych bez spreadu otrzymują syntetyczne ceny kupna i sprzedaży.
