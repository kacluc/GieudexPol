# Specyfikacja funkcjonalnosci: pobieranie kursow walut

## 1. Cel funkcjonalnosci

Celem funkcjonalnosci jest pobieranie kursow walut z zewnetrznych zrodel, zapisanie ich w lokalnej bazie danych i udostepnienie frontendowi jednolitego formatu danych do wykresu oraz tabel.

Frontend komunikuje sie tylko z backendem. Backend najpierw sprawdza baze danych. Jezeli brakuje danych dla wybranej waluty, zrodla i zakresu dat, backend synchronizuje brakujace dane z odpowiedniego zrodla, zapisuje je w bazie i dopiero wtedy zwraca odpowiedz.

Obslugiwane zrodla:

- `NBP` - Narodowy Bank Polski, tabela C, kurs kupna i sprzedazy.
- `ECB` - European Central Bank, oficjalny XML z kursami referencyjnymi.
- `MOCK_BANK_A` - lokalne dane developerskie seedowane w srodowisku Development.

## 2. Zasady danych

Wszystkie kursy zapisywane w tabeli `ExchangeRates` sa kursami wzgledem PLN.

### NBP

NBP publikuje kursy kupna i sprzedazy w tabeli C:

- `bid` -> `ExchangeRate.BuyPrice`,
- `ask` -> `ExchangeRate.SellPrice`,
- `effectiveDate` -> `ExchangeRate.EffectiveDate`.

### ECB

ECB publikuje kursy wzgledem EUR w pliku XML:

```text
https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml
```

Dane sa przeliczane do PLN przed zapisem:

```text
RateToPLN(currency) = EUR_PLN / EUR_CURRENCY
```

Przyklady:

- `EUR_PLN = 4.25`,
- `EUR_USD = 1.10`,
- `USD_PLN = 4.25 / 1.10`.

Dla `EUR`:

```text
RateToPLN(EUR) = EUR_PLN
```

ECB publikuje kurs referencyjny, a nie bid/ask, dlatego:

- `BuyPrice = RateToPLN`,
- `SellPrice = RateToPLN`.

Nie jest tworzony sztuczny spread.

## 3. Zakres dat

Endpointy odczytu i synchronizacji moga przyjmowac `from` oraz `to`.

Jezeli `from` lub `to` nie zostana podane, backend uzywa zakresu:

```text
from = 1 stycznia biezacego roku
to = DateTime.Today
```

ECB nie publikuje kursow w weekendy i swieta. Brak punktow dla takich dni nie jest bledem i nie jest uzupelniany sztucznie.

## 4. Przeplyw odczytu wykresu

1. Uzytkownik wybiera walute, zrodlo i zakres dat na `/rates`.
2. Frontend wywoluje:

```http
GET /api/ExchangeRates/chart?currency=USD&source=ECB&from=2026-01-01&to=2026-05-24
```

3. Backend odczytuje lokalna baze danych dla:

```text
Currency.Symbol = USD
RateSource.Code = ECB
EffectiveDate between from and to
```

4. Jezeli dane istnieja, backend zwraca `ExchangeRateChartResponseDto`.
5. Jezeli danych brakuje, backend wywoluje:

```text
SyncRatesAsync(source, from, to)
```

6. Synchronizacja pobiera dane z odpowiedniego klienta zewnetrznego:

- `NBP` -> `NbpExchangeRateClient`,
- `ECB` -> `EcbExchangeRateClient`.

7. Backend zapisuje brakujace kursy w `ExchangeRates`.
8. Backend ponownie odczytuje baze danych.
9. Backend zwraca dane do frontendu.

Wykres zawsze zawiera dane tylko z jednego wybranego zrodla.

## 5. Przeplyw latest

Endpoint:

```http
GET /api/ExchangeRates/latest?source=ECB&currency=USD
```

Backend:

1. Odczytuje najnowszy rekord z bazy dla zrodla i opcjonalnej waluty.
2. Jezeli brak danych albo brak danych z biezacego roku, uruchamia:

```text
SyncCurrentYearRatesAsync(source)
```

3. Po synchronizacji ponownie odczytuje baze.
4. Zwraca najnowszy dostepny dzien publikacji.

Brak rekordu z dzisiejsza data nie jest bledem, poniewaz zrodla nie publikuja kursow codziennie.

## 6. Endpointy

### Dane do wykresu

```http
GET /api/ExchangeRates/chart?currency=EUR&source=NBP&from=2026-01-01&to=2026-05-24
GET /api/ExchangeRates/chart?currency=USD&source=ECB
```

### Najnowsze kursy

```http
GET /api/ExchangeRates/latest?source=NBP
GET /api/ExchangeRates/latest?source=ECB&currency=USD
```

### Synchronizacja NBP

```http
POST /api/ExchangeRates/sync/nbp?from=2026-01-01&to=2026-05-24
```

### Synchronizacja ECB

```http
POST /api/ExchangeRates/sync/ecb
POST /api/ExchangeRates/sync/ecb?from=2026-01-01&to=2026-05-24
```

### Synchronizacja ogolna

```http
POST /api/ExchangeRates/sync/{sourceCode}
```

## 7. Model danych

Funkcjonalnosc uzywa wspolnych encji:

- `Currency`,
- `RateSource`,
- `ExchangeRate`.

Nie ma osobnych tabel `NbpRates` ani `EcbRates`.

Kazdy rekord kursu ma:

- `CurrencyId`,
- `RateSourceId`,
- `BuyPrice`,
- `SellPrice`,
- `EffectiveDate`,
- `FetchedAt`.

Unikalnosc logiczna:

```text
CurrencyId + RateSourceId + EffectiveDate
```

## 8. Najwazniejsze klasy

### API

- `ExchangeRatesController`
  - `GetChartData`,
  - `GetLatestRates`,
  - `SyncNbpRates`,
  - `SyncEcbRates`,
  - `SyncRatesBySource`.

### Application

- `IExchangeRateService`,
- `IExchangeRateSyncService`,
- `IExternalExchangeRateClient`,
- `IExchangeRateRepository`.

### Infrastructure

- `ExchangeRateSyncService`
  - wybiera klienta po `SourceCode`,
  - tworzy `RateSource`,
  - tworzy brakujace `Currency`,
  - zapisuje brakujace `ExchangeRate`,
  - pomija duplikaty.

- `NbpExchangeRateClient`
  - pobiera JSON z NBP tabela C,
  - mapuje `bid` i `ask` do wspolnego DTO.

- `EcbExchangeRateClient`
  - pobiera XML ECB,
  - parsuje `Cube time`, `currency`, `rate`,
  - przelicza kursy z EUR-relative na PLN-relative,
  - ustawia `BuyPrice = SellPrice`.

## 9. Diagramy PlantUML

Diagramy tej funkcjonalnosci sa trzymane w osobnym folderze, zeby nie mieszaly sie z ogolnymi diagramami systemu:

- `UML/KursyWalut/PobieranieKursowSequence.puml` - przeplyw synchronizacji i cache-miss dla NBP/ECB.
- `UML/KursyWalut/PobieranieKursowClassDiagram.puml` - klasy funkcjonalnosci kursow.
- `UML/KursyWalut/IntegracjaZrodelSequence.puml` - ogolny przeplyw integracji z dostawcami.
- `UML/KursyWalut/IntegracjaZrodelClassDiagram.puml` - ogolny diagram klas integracji.
- `UML/KursyWalut/PrzypadkiUzyciaPobieraniaKursow.puml` - przypadki uzycia dla podgladu kursow, cache-miss i synchronizacji zrodel.
