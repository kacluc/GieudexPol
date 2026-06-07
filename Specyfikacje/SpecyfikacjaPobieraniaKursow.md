# Specyfikacja obslugi kursow walut

## 1. Cel i zakres

Modul kursow walut:

- pobiera dane z bankow centralnych,
- normalizuje kursy do PLN za 1 jednostke waluty,
- zapisuje historie w SQL Server,
- udostepnia wykres i tabele przez API,
- synchronizuje brakujace lub nieaktualne dane,
- zachowuje jedna konwencje `BuyPrice`, `SellPrice` i `MidPrice`.

Frontend nie laczy sie bezposrednio z dostawcami. Cala komunikacja ze zrodlami
zewnetrznymi przechodzi przez backend.

## 2. Dostepne zrodla

| Kod | Dostawca | Format | Charakter kursu | Maks. zakres jednego wywolania |
| --- | --- | --- | --- | --- |
| `NBP` | Narodowy Bank Polski, tabela C | JSON | realny bid/ask | 93 dni |
| `ECB` | European Central Bank | XML | kurs referencyjny wzgledem EUR | 366 dni |
| `RIKSBANK` | Sveriges Riksbank, grupa 130 | JSON REST | kurs referencyjny wzgledem SEK | 366 dni |
| `BOE` | Bank of England Database | CSV | publikowany kurs spot wzgledem GBP | 366 dni |
| `CNB` | Czech National Bank | JSON | kurs referencyjny wzgledem CZK | 31 dni |
| `NORGES` | Norges Bank | SDMX-JSON | kurs referencyjny wzgledem NOK | 366 dni |
| `BNR` | National Bank of Romania | XML | kurs referencyjny wzgledem RON | 366 dni |
| `MOCK_BANK_A` | dane deweloperskie | baza lokalna | dane testowe | brak synchronizacji HTTP |

Bazowe adresy API sa konfigurowane w `GieudexPol.API/appsettings.json`.

## 3. Obslugiwane waluty

Panel udostepnia:

```text
AUD, CAD, CHF, CZK, DKK, EUR, GBP, HUF,
JPY, KRW, NOK, RON, SEK, TRY, USD
```

NBP tabela C nie publikuje w panelu `KRW`, `RON` i `TRY`. Dostepnosc waluty
jest zatem filtrowana osobno dla wybranego zrodla.

`PLN` moze byc pobierany technicznie przez klienta dostawcy w celu przeliczenia,
ale nie jest zapisywany jako kurs waluty obcej do PLN.

## 4. Konwencja cen

Backend przechowuje perspektywe banku lub kantoru:

- `BuyPrice` - system kupuje walute od uzytkownika; wartosc nizsza,
- `SellPrice` - system sprzedaje walute uzytkownikowi; wartosc wyzsza,
- `MidPrice` - kurs referencyjny lub srodek realnego bid/ask.

Warunek biznesowy:

```text
BuyPrice < SellPrice
```

Frontend tlumaczy pola na perspektywe uzytkownika:

- `Sprzedajesz walute po` = `BuyPrice`,
- `Kupujesz walute po` = `SellPrice`,
- `Kurs referencyjny` = `MidPrice`.

## 5. NBP tabela C

NBP jest jedynym zrodlem z realnym bid/ask:

```text
BuyPrice = bid
SellPrice = ask
MidPrice = round((bid + ask) / 2, 4)
```

Endpoint:

```text
GET https://api.nbp.pl/api/exchangerates/tables/C/{from}/{to}/?format=json
```

NBP nie przechodzi przez kalkulator sztucznego spreadu.

## 6. Zrodla referencyjne

ECB, RIKSBANK, BOE, CNB, NORGES i BNR nie publikuja oficjalnej tabeli
kupna/sprzedazy zgodnej z modelem aplikacji. Klient dostawcy najpierw wyznacza
kurs referencyjny w PLN, a synchronizator tworzy ceny syntetyczne.

### 6.1 Normalizacja do PLN

| Zrodlo | Normalizacja |
| --- | --- |
| ECB | `EUR_PLN / EUR_CURRENCY` |
| RIKSBANK | `SEK_CURRENCY / SEK_PLN`; dla SEK: `1 / SEK_PLN` |
| BOE | `GBP_PLN / GBP_CURRENCY`; dla GBP: `GBP_PLN` |
| CNB | `(RATE_CZK / AMOUNT) / PLN_TO_CZK`; dla CZK: `1 / PLN_TO_CZK` |
| NORGES | `(RATE_NOK / UNIT) / PLN_TO_NOK`; dla NOK: `1 / PLN_TO_NOK` |
| BNR | `(RATE_RON / MULTIPLIER) / PLN_TO_RON`; dla RON: `1 / PLN_TO_RON` |

Po normalizacji klient ustawia `ReferenceRate`. Pole jest przekazywane do
`ExchangeRateSyncService`.

### 6.2 Sztuczny spread

Wspolna logika znajduje sie w `ExchangeRateSpreadCalculator`:

```text
halfSpread = SpreadPercent / 2
BuyPrice = round(ReferenceRate * (1 - halfSpread), 4)
SellPrice = round(ReferenceRate * (1 + halfSpread), 4)
MidPrice = round(ReferenceRate, 4)
```

Domyslna konfiguracja:

```json
"ExchangeRateSettings": {
  "SyntheticSpreadPercent": 0.02
}
```

`0.02` oznacza calkowity spread 2%, czyli 1% ponizej i 1% powyzej kursu
referencyjnego.

Dla bardzo niskich kursow, np. KRW, zaokraglenie do 4 miejsc moze zrownac
obie ceny. W takim przypadku kalkulator stosuje minimalny krok `0.0001`,
aby nadal zachowac `BuyPrice < SellPrice`.

## 7. Model danych

Encja `ExchangeRate` zawiera:

- `CurrencyId`,
- `RateSourceId`,
- `BuyPrice`,
- `SellPrice`,
- `MidPrice`,
- `EffectiveDate`,
- `FetchedAt`.

Unikalny klucz logiczny:

```text
CurrencyId + RateSourceId + EffectiveDate
```

`RateSource.Code` rozroznia dostawcow. Wszystkie wartosci w `ExchangeRates`
oznaczaja PLN za 1 jednostke waluty.

## 8. Zakres dat

Panel i API przyjmuja zakres:

```text
2026-01-01 <= from <= to <= dzisiaj
```

Jesli `from` lub `to` nie sa podane:

```text
from = 1 stycznia biezacego roku
to = DateTime.Today
```

Frontend oferuje presety `7D`, `30D`, `90D`, `YTD` i `DEV`.

W bazie i na wykresie wystepuja tylko dni opublikowane przez dostawce.
Weekendy, swieta i inne dni bez publikacji nie sa uzupelniane sztucznie.
Ostatni dostepny dzien moze byc wczesniejszy niz dzisiaj.

## 9. Pobieranie i synchronizacja

### 9.1 Odczyt wykresu

1. Frontend wysyla `GET /api/ExchangeRates/chart`.
2. Backend waliduje walute, zrodlo i daty.
3. `ExchangeRateService` odczytuje lokalne rekordy.
4. Synchronizacja jest uruchamiana, gdy:
   - brak punktow,
   - brak oczekiwanej daty publikacji,
   - zrodlo referencyjne ma stare rekordy z `BuyPrice == SellPrice`.
5. `ExchangeRateSyncService` wybiera klienta po `SourceCode`.
6. Zakres jest dzielony wedlug `MaxRangeDays`.
7. Klient pobiera i normalizuje dane do PLN.
8. Synchronizator zachowuje realne NBP albo generuje spread syntetyczny.
9. Nowe rekordy sa dodawane, a istniejace rekordy syntetyczne odswiezane.
10. Backend ponownie odczytuje baze i zwraca punkty wykresu.

Synchronizacja jest blokowana osobnym `SemaphoreSlim` dla kazdego kodu zrodla.
Konflikt unikalnego klucza po rownoleglym zapisie powoduje ponowna probe po
wyczyszczeniu trackingu EF.

### 9.2 Odczyt najnowszych kursow

`GET /api/ExchangeRates/latest` zwraca najnowszy dzien osobno dla kazdej waluty
z wybranego zrodla. Synchronizacja biezacego roku jest uruchamiana, gdy brak
danych, dane sa sprzed biezacego roku albo wykryto stare rowne ceny syntetyczne.

### 9.3 Synchronizacja przy starcie

`ExchangeRateStartupSyncService`:

1. czeka na SQL Server,
2. wykonuje migracje EF Core,
3. w Development uruchamia seeder,
4. sprawdza kolejno `NBP`, `ECB`, `RIKSBANK`, `BOE`, `CNB`, `NORGES`, `BNR`,
5. pobiera brakujacy zakres do dzisiaj,
6. naprawia historyczne syntetyczne rekordy z rownymi cenami.

Oczekiwana data publikacji cofa sobote i niedziele do piatku. Dostawca moze
jednak nie opublikowac danych takze z powodu lokalnego swieta.

## 10. Endpointy

### Odczyt

```http
GET /api/ExchangeRates/chart?currency=EUR&source=NBP&from=2026-01-01&to=2026-06-07
GET /api/ExchangeRates/chart?currency=USD&source=ECB
GET /api/ExchangeRates/latest?source=BNR
GET /api/ExchangeRates/latest?source=BOE&currency=USD
```

### Synchronizacja dedykowana

```http
POST /api/ExchangeRates/sync/nbp?from=2026-01-01&to=2026-06-07
POST /api/ExchangeRates/sync/ecb
POST /api/ExchangeRates/sync/riksbank
POST /api/ExchangeRates/sync/boe
POST /api/ExchangeRates/sync/cnb
POST /api/ExchangeRates/sync/norges
POST /api/ExchangeRates/sync/bnr
```

### Synchronizacja ogolna

```http
POST /api/ExchangeRates/sync/{sourceCode}?from=2026-01-01&to=2026-06-07
```

Brak dat w endpointach synchronizacji oznacza biezacy rok do dzisiaj.

## 11. Odpowiedz i prezentacja

Punkt wykresu zawiera:

```json
{
  "date": "2026-06-05",
  "buyPrice": 4.2075,
  "midPrice": 4.2500,
  "sellPrice": 4.2925
}
```

Panel `/rates` wyswietla:

- wykres linii `BuyPrice` i `SellPrice`,
- dymek dla wybranego dnia,
- liczbe dni publikacji,
- najnowsze ceny dla wybranej waluty,
- tabele historii z `MidPrice` i spreadem,
- tabele najnowszych kursow wszystkich dostepnych walut.

Wykres zawsze pokazuje jedno wybrane zrodlo. Kursy roznych dostawcow nie sa
laczone w jedna serie.

## 12. Obsluga bledow

- `from > to`, `from < 2026-01-01` lub `to > dzisiaj` zwraca `400`.
- Nieobslugiwany kod synchronizacji zwraca `400`.
- Brak publikacji dla zakresu daje pusta liste lub ostrzezenie synchronizacji.
- Brak technicznego kursu PLN potrzebnego do normalizacji powoduje czytelny
  `InvalidOperationException` i ostrzezenie dla danego zakresu.
- Blad jednego zrodla przy starcie nie zatrzymuje sprawdzania pozostalych.

## 13. Diagramy PlantUML

- `PobieranieKursowSequence.puml` - odczyt wykresu i automatyczny cache-miss.
- `PobieranieKursowActivity.puml` - decyzje walidacji, synchronizacji i spreadu.
- `PobieranieKursowClassDiagram.puml` - glowne klasy calego modulu.
- `IntegracjaZrodelSequence.puml` - pobieranie, normalizacja i zapis dostawcy.
- `IntegracjaZrodelClassDiagram.puml` - klienci zewnetrzni i kalkulator spreadu.
- `PrzypadkiUzyciaPobieraniaKursow.puml` - przypadki uzycia uzytkownika i systemu.
