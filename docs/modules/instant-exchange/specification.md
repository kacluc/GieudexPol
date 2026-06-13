# Szybka wymiana

## Cel

Użytkownik wskazuje walutę źródłową, docelową i kwotę. System sam wybiera aktywne źródło kursu z najlepszym wynikiem i wystarczającą płynnością.

## Serwisy

- `WalletService`;
- `InstantExchangeService`;
- `TransactionFeeCalculator`;
- `SystemAccountService`.

## Wykonanie

Realna wymiana przenosi środki między użytkownikiem i kontem systemowym źródła, przekazuje fee do PlatformTreasury, tworzy `ExchangeExecution` i dwa wpisy `Transaction`.

## Preview

Preview wywołuje wspólne `BuildQuoteAsync` i tę samą selekcję oferty. Zwraca wynik, kurs, fee, datę i informację o środkach użytkownika. Nie wywołuje `SaveChanges`, nie tworzy wykonania ani transakcji.

## Co pokazać

Przypadek, w którym źródło z najlepszym kursem odpada przez brak waluty docelowej i wybierane jest następne.
