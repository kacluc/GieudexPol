# Specyfikacja GieudexPol

Ten plik jest indeksem aktualnej specyfikacji. Szczegóły zostały podzielone według modułów, aby nie mieszać wymagań historycznych ze stanem kodu.

## Stan aplikacji

GieudexPol składa się z Angularowego SPA, ASP.NET Core Web API na .NET 8 i bazy SQL Server. Użytkownik może zarządzać portfelami, wykonywać operacje finansowe, korzystać z automatycznie wybranego źródła szybkiej wymiany, składać zlecenia limitowe oraz definiować alerty kursowe i rynkowe. Worker w tle synchronizuje dane startowe i cyklicznie ocenia alerty.

## Specyfikacje modułowe

1. [Kursy walut](docs/modules/exchange-rates/specification.md)
2. [Portfele](docs/modules/wallets/specification.md)
3. [Szybka wymiana](docs/modules/instant-exchange/specification.md)
4. [Rynek walut](docs/modules/order-book/specification.md)
5. [Prowizje i skarbiec](docs/modules/fees-and-treasury/specification.md)
6. [Konta systemowe](docs/modules/system-accounts/specification.md)
7. [Alerty](docs/modules/alerts/specification.md)
8. [Użytkownicy i autoryzacja](docs/modules/users-and-auth/specification.md)
9. [Administracja](docs/modules/admin/specification.md)

## Najważniejsze aktualne reguły

- `Role` steruje autoryzacją, a `AccountType` opisuje biznesowy charakter konta.
- Wszystkie operacje finansowe korzystają z centralnego kalkulatora: `max(0,5% kwoty, równowartość 10 PLN)`.
- Prowizje są księgowane na portfele konta `PlatformTreasury`.
- Szybka wymiana wybiera najlepsze aktywne źródło z kursem nie starszym niż 7 dni i wystarczającą płynnością.
- Preview wymiany używa tej samej selekcji i kalkulatora prowizji, ale nie zapisuje zmian.
- Rynek obsługuje wyłącznie zlecenia limitowe, rezerwacje, częściowe wykonania i anulowanie.
- Alerty mają stany `Active`, `Fulfilled`, `Inactive`; `Fulfilled` nadal jest monitorowany.
- Konta źródeł i PlatformTreasury nie mogą logować się i są ukrywane przed zwykłymi listami użytkowników.

## Funkcje niezaimplementowane

Nie należy przedstawiać jako gotowych: market orders, stop loss, margin, dźwigni, SignalR, e-mail/push alerts, arbitrażu automatycznego, konfiguratora prowizji w panelu admina ani raportów finansowych.

## Uwagi / do weryfikacji

- Część starszych kontrolerów CRUD (`Currencies`, `ExchangeRates`, `Wallets`) jest chroniona tylko `[Authorize]`, nie osobną rolą administratora.
- Endpoint historii nadal zawiera `userId` w trasie, ale backend porównuje go z użytkownikiem JWT.
- `SuperAdmin` istnieje w modelu i JWT, lecz kontrolery admina wymagają obecnie roli `Admin`; nie ma osobnego zestawu endpointów SuperAdmin.
- Starsze materiały w `UML/` i `Specyfikacje/` pozostają w repozytorium jako dokumenty historyczne. Aktualnym źródłem dokumentacji jest folder `docs/`.
