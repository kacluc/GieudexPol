# Kursy walut

## Cel

Moduł pobiera, zapisuje i udostępnia dzienne kursy walut względem PLN. Dostarcza dane wykresom, alertom, kalkulatorowi prowizji oraz szybkiej wymianie.

## Encje i serwisy

- `Currency`, `RateSource`, `ExchangeRate`;
- `ExchangeRateService`, `ExchangeRateSyncService`;
- klienci NBP, ECB, Riksbank, BOE, BOC, CNB, Norges i BNR;
- `ExchangeRateStartupSyncService`;
- `AdminTestExchangeRateService`.

## Scenariusze

1. API odczytuje lokalne dane dla źródła i okresu.
2. Brak lub niekompletność danych może uruchomić synchronizację.
3. Klient źródła pobiera dane i normalizuje je względem PLN.
4. Rekord jest zapisywany z datą efektywną i czasem pobrania.
5. Alerty i szybka wymiana korzystają z zapisanych danych.

Źródła mockowe `MOCK_BANK_A` i `MOCK_BANK_B` służą administratorom. Seeder nie utrzymuje dla nich kursów z bieżącego ani przyszłego dnia.

## Co pokazać

Wykres jednego źródła, ręczną synchronizację, różnicę `BuyPrice`/`SellPrice`, panel danych testowych i wykorzystanie tego samego kursu przez preview wymiany.
