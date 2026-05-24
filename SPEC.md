# SPECIFICATION: GieudexPol Trading Engine (v1.0)

## 🎯 1. Project Overview & Goals

*   **Project Name:** GieudexPol
*   **Mission Statement:** To maximize profit from market fluctuations by providing advanced live data analytics and arbitrage detection capabilities.
*   **Core Functionality:** Automated aggregation of foreign exchange rates from multiple sources (e.g., NBP, commercial APIs) to identify real-time pricing discrepancies for profitable trading opportunities.
*   **Key Objectives:**
    *   **Data Aggregation:** Seamless fetching of live currency rates from diverse external APIs.
    *   **Margin Engine:** Real-time calculation and application of transaction commissions/margins on base rates.
    *   **Anomaly Detection:** Implementing an algorithm to flag price errors or sudden, significant rate spikes.
    *   **Notification Speed:** Minimizing latency between opportunity detection and user notification.

## 🏗️ 2. Architecture & Technology Stack

*   **Architecture Pattern:** Clean Architecture (Ensuring separation of concerns: Domain -> Application -> Infrastructure).
*   **Technology Stack:**
    *   **Frontend:** Angular 21 (Reactive client interface).
    *   **Backend:** .NET 10 WebAPI.
    *   **Database:** MS SQL Server (Relational data storage for history and profiles).
    *   **AI/Agent Support:** Cline (Gemini-2.5-flash) Agent, utilized for code development assistance and auditing via an Agentic Loop methodology.
    *   **Documentation:** PlantUML (For architectural diagrams).

### 📂 Directory Structure:
*   `GieudexPol.Domain`: Core entities (`Currency`, `Rate`, `Alert`) and business rules.
*   `GieudexPol.Application`: Business services (BLL), calculation logic, and notification interfaces.
*   `GieudexPol.Infrastructure`: Data Access Layer (DAL) implementation using Entity Framework Core; handles external API integration (e.g., bank APIs).
*   `GieudexPol.API`: REST Controllers and Middleware (JWT, CORS) serving as the primary entry point for the Angular frontend.
*   `GieudexPol.Frontend/src/app`: Angular 21 application with Clean Architecture pattern

### 🖥️ Frontend Architecture (Angular 21)

The frontend follows a modular architecture organized within `src/app/` directory:

```
src/app/
├── core/                  # Core module (singleton services)
│   ├── services/          # Global services (Auth, API clients)
│   ├── interceptors/      # HTTP interceptors
│   ├── guards/            # Route guards
│   ├── models/            # Global DTOs and interfaces
│   └── core.config.ts     # Global configuration
│
├── shared/                # Shared components and utilities
│   ├── components/        # Reusable UI components
│   ├── directives/        # Custom directives
│   ├── pipes/             # Custom pipes
│   ├── utils/             # Utility functions
│   └── shared.module.ts   # Shared module (standalone)
│
├── features/              # Feature modules (lazy-loaded)
│   ├── auth/              # Authentication
│   │   ├── components/    # Auth-specific components
│   │   ├── services/      # Auth services
│   │   ├── models/        # Auth models
│   │   └── auth.routes.ts # Auth routing
│   │
│   ├── wallet/            # Wallet management
│   │   ├── components/    # Wallet components
│   │   ├── services/      # Wallet services
│   │   ├── models/        # Wallet models
│   │   └── wallet.routes.ts
│   │
│   ├── rates/             # Exchange rates
│   │   ├── components/    # Rate components
│   │   ├── services/      # Rate services
│   │   ├── models/        # Rate models
│   │   └── rates.routes.ts
│   │
│   └── admin/             # Admin panel
│
├── layouts/               # Layout templates
│   ├── main-layout/       # Main application layout
│   └── auth-layout/       # Authentication layout
│
├── app.config.ts          # Application configuration
├── app.routes.ts          # Main routing with lazy loading
└── app.component.ts       # Root component (standalone)
```

### Frontend Development Guidelines

1. **Naming Conventions**:
   - Folders: `kebab-case` (e.g., `wallet-dashboard`)
   - Components: `PascalCase` with `.component.ts` suffix (e.g., `WalletDashboardComponent`)
   - Services: `camelCase` with `.service.ts` suffix (e.g., `wallet.service.ts`)
   - Models/Interfaces: `PascalCase` (e.g., `WalletBalance.interface.ts`)

2. **Module Organization**:
   - Each feature module contains: components/, services/, models/, and routes.ts
   - Components are organized by functionality in subfolders
   - All components are `standalone: true` (no NgModules)

3. **State Management**:
   - Uses Angular 21 Signals (`signal()`, `computed()`, `effect()`)
   - Zoneless architecture (no Zone.js)
   - Signal-based forms for reactive form handling

4. **API Integration**:
   - Services in `features/[module]/services/` handle API communication
   - Use injected HttpClient with proper typing
   - Follow backend DTO contracts exactly

## 💾 3. Data Model & Entities (MS SQL Server Schema)

*   **Users:** Stores profile data, hashed passwords (BCrypt/Identity), and user roles (Admin/User).
*   **Wallets:** Tracks the current balance of specific currencies for a given user.
*   **Currencies:** Defines all managed assets (Symbol, Name, Activity Status).
*   **ExchangeRates:** Historical records of rates (Buy/Sell) with high precision (`decimal(18,4)`).
*   **Transactions:** Immutable ledger of all operations (transfers, buy/sell), including applied fees and current status.
*   **UserAlerts:** Configuration for user-defined price thresholds and associated currencies.

## 🚀 4. System Scope & Functionalities

### A. User Features (Client Facing)
1.  **Authentication:** Full registration, login mechanism with server-side validation. Dashboard defaults to the login screen.
2.  **Digital Wallet:** Display of current balance and available funds for trading.
3.  **Order Placement:** Intuitive form for market buy/sell orders at prevailing rates.
4.  **Orderbook:** Real-time display of active user bids and asks.
5.  **Interactive Charts:** Visualization tools for price trend analysis.
6.  **Transaction History:** Comprehensive, auditable log of all operations (deposits, trades).
7.  **Price Alerts:** System to notify users when an asset reaches a specified target price.
8.  **Wallet Management (Portfel):** Centralizowany moduł do kompleksowego zarządzania środkami użytkownika, obejmujący wymianę walut, wpłaty i wypłaty.
    *   **Cel:** Umożliwienie użytkownikowi bezpiecznego zarządzania środkami, wymiany walut oraz dokonywania wpłat i wypłat w ramach ekosystemu GieudexPol.
    *   **Interfejs użytkownika:** Kompaktowy układ z zakładkami (tabs) umożliwiający łatwe przełączanie między funkcjonalnościami bez konieczności scrollowania.
    *   **Endpoint API (GET):** `/api/wallets/user/{userId}`
        *   **Opis:** Pobiera aktualne salda portfela dla wszystkich walut powiązanych z użytkownikiem, zwracając listę obiektów `Wallet` zawierającą unikalne saldo dla każdej waluty (`CurrencyId`).
    *   **Endpoint API (POST):** `/api/wallets/trade`
        *   **Opis:** Realizuje transakcję handlową poprzez obciążenie portfela źródłowego i zasilenie portfela docelowego.
        *   **Payload (`TradeRequest`):** Wymaga następujących pól: `userId`, `FromCurrencyId`, `AmountFrom` (kwota sprzedawana), `ToCurrencyId`, `AmountTo` (kwota kupowana).
        *   **Logika Biznesowa:** System wykonuje dwuetapową operację księgową: najpierw debetuje salda źródłowe, a następnie kredytuje saldo docelowe. Po pomyślnej transakcji generowane są dwa rekordy audytowe w tabeli `Transactions`:
            *   **Sprzedaż (Sell):** Rejestruje sprzedaną walutę i ilość.
            *   **Zakup (Buy):** Rejestruje zakupioną walutę i ilość, wraz z wyliczeniem umownego kursu/ceny dla obu operacji.
    *   **Endpoint API (POST):** `/api/wallets/deposit`
        *   **Opis:** Realizuje wpłatę środków na portfel użytkownika.
        *   **Payload (`DepositRequest`):** Wymaga pól: `userId`, `CurrencyId`, `Amount`.
        *   **Logika Biznesowa:** System kredytuje wskazany portfel użytkownika i rejestruje transakcję typu "Deposit" w tabeli `Transactions`.
    *   **Endpoint API (POST):** `/api/wallets/withdraw`
        *   **Opis:** Realizuje wypłatę środków z portfela użytkownika.
        *   **Payload (`WithdrawRequest`):** Wymaga pól: `userId`, `CurrencyId`, `Amount`.
        *   **Logika Biznesowa:** System waliduje dostępne środki, debetuje wskazany portfel i rejestruje transakcję typu "Withdrawal" w tabeli `Transactions`. W przypadku niewystarczających środków zwraca błąd "Niewystarczające środki na koncie".
    *   **Przekierowanie do transferów:** Przycisk umożliwiający przejście do komponentu `transaction-transfer` w celu realizacji transferów między użytkownikami.

### B. Administrator Features (Management)
1.  **User Management:** Full CRUD capabilities for user profiles (blocking, deletion, password reset).
2.  **Commission Configuration:** Global setting for percentage-based transaction fees.
3.  **Market Management:** Ability to add new trading pairs and temporarily suspend markets.
4.  **Security Monitoring:** Viewing system logs and detecting suspicious activity/intrusion attempts.
5.  **Financial Reporting:** Generating reports on total turnover volume and platform profit.

## ⚙️ 5. Deployment & Setup Instructions (DevOps)

*   **Deployment Platform:** Railway.app (Production URL: [URL]).
*   **Local Setup Method:** Docker Compose (Recommended for isolated environment).
    1.  `git clone ...`
    2.  `docker-compose up -d` (Builds all necessary services: DB, API, Frontend).
*   **Database Initialization:** Use `dotnet ef database update` to apply migrations and seed initial data if needed.

## 6. Exchange Rate Source Integration

### Purpose

The exchange rate module imports currency rates from external providers, stores them in SQL Server, and exposes one consistent API shape to the Angular `/rates` view. The frontend never calls NBP, ECB or Riksbank directly. It asks the backend, and the backend decides whether data can be read from the local database or must be synchronized first.

Supported sources:

*   `NBP` - Narodowy Bank Polski, table `C`, buy and sell rates.
*   `ECB` - European Central Bank, official XML reference rates.
*   `RIKSBANK` - Sveriges Riksbank, official REST API reference rates against SEK.
*   `MOCK_BANK_A` - development seed data used as a mock bank source.

### Main Design Rules

*   External sources implement the shared `IExternalExchangeRateClient` contract.
*   `ExchangeRateSyncService` receives `IEnumerable<IExternalExchangeRateClient>` and selects a client by `SourceCode`.
*   Rates from every source are stored in the same `ExchangeRates` table.
*   Source identity is stored through `RateSource`.
*   The logical duplicate key is `CurrencyId + RateSourceId + EffectiveDate`.
*   Chart data for a selected source contains only rows from that source.
*   All persisted and returned rates are PLN-relative.

### NBP Source

NBP uses table `C`:

```text
https://api.nbp.pl/api/exchangerates/tables/C/{from}/{to}/?format=json
```

Mapping:

*   `code` -> `Currency.Symbol`
*   `currency` -> `Currency.Name`
*   `bid` -> `ExchangeRate.BuyPrice`
*   `ask` -> `ExchangeRate.SellPrice`
*   `effectiveDate` -> `ExchangeRate.EffectiveDate`

### ECB Source

ECB uses the official XML file:

```text
https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml
```

The XML is parsed from `Cube` elements:

*   day node: `Cube time="yyyy-MM-dd"`
*   rate node: `Cube currency="USD" rate="1.10"`

ECB publishes rates relative to EUR. Before saving anything to `ExchangeRates`, the backend converts each value to PLN-relative:

```text
RateToPLN(currency) = EUR_PLN / EUR_CURRENCY
```

Example:

```text
1 EUR = 4.25 PLN
1 EUR = 1.10 USD
1 USD = 4.25 / 1.10 PLN
```

Special cases:

*   `RateToPLN(EUR) = EUR_PLN`
*   If a given ECB day has no `PLN` rate, that day is skipped.
*   ECB does not publish bid/ask, so `BuyPrice = RateToPLN` and `SellPrice = RateToPLN`.
*   No artificial spread is created for ECB.

ECB does not publish on weekends and holidays. Missing weekend points are not filled artificially.

### Riksbank Source

Riksbank uses the official SWEA REST API:

```text
https://api.riksbank.se/swea/v1/Observations/ByGroup/130/{from}/{to}
```

Group `130` contains currencies against Swedish kronor. The client keeps only the currencies supported by the application: `EUR`, `USD`, `CHF`, `GBP`, `HUF`, `CZK`, `DKK`, `SEK`, `NOK`, `RON`, `TRY`, `AUD`, `CAD`, `JPY`, `KRW`, plus `PLN` only as the conversion basis.

Riksbank values are quoted as SEK per 1 unit of foreign currency, for example:

```text
SEK_USD = 9.37
SEK_PLN = 2.56
```

Before saving anything to `ExchangeRates`, the backend converts each value to PLN-relative:

```text
RateToPLN(currency) = SEK_CURRENCY / SEK_PLN
RateToPLN(SEK) = 1 / SEK_PLN
```

Example:

```text
1 USD = 9.37 SEK
1 PLN = 2.56 SEK
1 USD = 9.37 / 2.56 PLN
```

Riksbank publishes indicative mid-market rates, not bid/ask, so `BuyPrice = RateToPLN` and `SellPrice = RateToPLN`. No artificial spread is created.

### Default Date Range

If `from` or `to` is not provided, the backend uses:

```csharp
from = new DateTime(DateTime.Today.Year, 1, 1);
to = DateTime.Today;
```

This applies to chart data, latest-rate cache misses, and manual ECB/Riksbank synchronization.

### Cache-Miss Flow

For chart data:

1.  Frontend calls `/api/ExchangeRates/chart`.
2.  Backend reads `ExchangeRates` for selected `currency + source + date range`.
3.  If local points exist, backend returns them.
4.  If points are missing and the source is syncable (`NBP`, `ECB` or `RIKSBANK`), backend calls `SyncRatesAsync(source, from, to)`.
5.  Synchronization fetches missing external data, stores it in `ExchangeRates`, then the backend reads the database again.
6.  Backend returns only rows from the selected source.

For latest data:

1.  Backend reads the newest local row for selected source and optional currency.
2.  If there is no data or no data from the current year, backend calls `SyncCurrentYearRatesAsync(source)`.
3.  Backend returns the newest available publication day. Today does not have to exist.

### Backend Endpoints

```http
GET /api/ExchangeRates/chart?currency=EUR&source=NBP&from=2026-01-01&to=2026-05-24
GET /api/ExchangeRates/chart?currency=USD&source=ECB
GET /api/ExchangeRates/chart?currency=USD&source=RIKSBANK
GET /api/ExchangeRates/latest?source=NBP
GET /api/ExchangeRates/latest?source=ECB&currency=USD
GET /api/ExchangeRates/latest?source=RIKSBANK&currency=USD
POST /api/ExchangeRates/sync/nbp?from=2026-01-01&to=2026-05-24
POST /api/ExchangeRates/sync/ecb
POST /api/ExchangeRates/sync/ecb?from=2026-01-01&to=2026-05-24
POST /api/ExchangeRates/sync/riksbank
POST /api/ExchangeRates/sync/riksbank?from=2026-01-01&to=2026-05-24
POST /api/ExchangeRates/sync/{sourceCode}
```

### Configuration

```json
{
  "NbpApi": {
    "BaseUrl": "https://api.nbp.pl/api/"
  },
  "EcbApi": {
    "BaseUrl": "https://www.ecb.europa.eu/stats/eurofxref/"
  },
  "RiksbankApi": {
    "BaseUrl": "https://api.riksbank.se/swea/v1/"
  },
  "NbpSync": {
    "StartDate": "2026-01-01"
  }
}
```

Docker Compose passes the same values through environment variables:

```text
NbpApi__BaseUrl=https://api.nbp.pl/api/
EcbApi__BaseUrl=https://www.ecb.europa.eu/stats/eurofxref/
RiksbankApi__BaseUrl=https://api.riksbank.se/swea/v1/
NbpSync__StartDate=2026-01-01
```

### PlantUML Documentation

Detailed diagrams for exchange-rate downloading are isolated in:

```text
UML/KursyWalut/
```

Relevant files:

*   `Specyfikacje/SpecyfikacjaPobieraniaKursow.md`
*   `UML/KursyWalut/PobieranieKursowSequence.puml`
*   `UML/KursyWalut/PobieranieKursowClassDiagram.puml`
*   `UML/KursyWalut/IntegracjaZrodelSequence.puml`
*   `UML/KursyWalut/IntegracjaZrodelClassDiagram.puml`
*   `UML/KursyWalut/PrzypadkiUzyciaPobieraniaKursow.puml`
