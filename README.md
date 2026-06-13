# GieudexPol

GieudexPol jest aplikacją do zarządzania portfelami walutowymi, obserwowania kursów, szybkiej wymiany z kontami płynności źródeł oraz handlu limitowego na wewnętrznym rynku użytkowników.

## Aktualny stos

- backend: ASP.NET Core Web API, .NET 8, Entity Framework Core 8;
- frontend: Angular 21, TypeScript 5.9, RxJS;
- baza: Microsoft SQL Server 2022;
- autoryzacja: JWT i role `User`, `Admin`, `SuperAdmin`;
- uruchamianie: Docker Compose albo procesy lokalne;
- testy: xUnit/Moq/EF Core InMemory oraz Vitest przez Angular CLI;
- diagramy: standardowy PlantUML w plikach `.puml`.

## Główne moduły

- [kursy walut](docs/modules/exchange-rates/specification.md);
- [portfele](docs/modules/wallets/specification.md);
- [szybka wymiana i symulator](docs/modules/instant-exchange/specification.md);
- [rynek walut i arkusz zleceń](docs/modules/order-book/specification.md);
- [prowizje i PlatformTreasury](docs/modules/fees-and-treasury/specification.md);
- [konta systemowe](docs/modules/system-accounts/specification.md);
- [alerty kursowe i rynkowe](docs/modules/alerts/specification.md);
- [użytkownicy i autoryzacja](docs/modules/users-and-auth/specification.md);
- [panel administratora](docs/modules/admin/specification.md).

Pełny indeks znajduje się w [docs/README.md](docs/README.md), a skrócona specyfikacja w [SPEC.md](SPEC.md).

## Szybkie uruchomienie

### Docker Compose

```powershell
docker compose up --build
```

Usługi z bieżącego `docker-compose.yml`:

- frontend przez nginx: `http://localhost`;
- API: `http://localhost:5010`;
- SQL Server: `localhost:1433`.

Zatrzymanie bez usuwania danych:

```powershell
docker compose down
```

Nie używaj `docker compose down -v`, jeżeli chcesz zachować wolumen bazy.

### Lokalnie

```powershell
docker compose up -d gieudexpol-db
dotnet run --project GieudexPol.API
cd GieudexPol.Frontend
npm install
npm start
```

Szczegóły: [docs/deployment/local-development.md](docs/deployment/local-development.md).

## Stan implementacji

Projekt obsługuje kursy wielu banków centralnych, portfele z rezerwacjami, prowizje i skarbiec platformy, szybką wymianę z kontrolą płynności, limitowy arkusz zleceń, alerty pracujące w tle oraz panele administracyjne. System nie implementuje market orders, dźwigni, margin tradingu ani zewnętrznego order booka.
