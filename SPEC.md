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

## 6. External Exchange Rate Integration

### Purpose

The exchange rate module is responsible for importing buy and sell currency rates from external providers, storing them in the local SQL Server database, and exposing the stored data to the Angular frontend. The frontend does not call external providers directly. It communicates only with the GieudexPol backend.

The first supported external provider is:

*   `NBP` - Narodowy Bank Polski, using table `C`, because table `C` contains buy (`bid`) and sell (`ask`) rates.

The implementation is intentionally provider-oriented instead of NBP-only. External providers are represented by the common `IExternalExchangeRateClient` interface. NBP is currently one implementation: `NbpExchangeRateClient`.

### Main Design Rules

*   External exchange rate providers are hidden behind `IExternalExchangeRateClient`.
*   Provider-specific HTTP details stay in Infrastructure, for example `NbpExchangeRateClient`.
*   Synchronization logic is handled by `ExchangeRateSyncService`.
*   Rates are saved to `ExchangeRates` and linked to `Currencies` and `RateSources`.
*   Frontend reads rates only from backend endpoints.
*   Local database is the source of truth for charts and latest-rate tables.
*   Duplicate rates are skipped by the unique key: `CurrencyId + RateSourceId + EffectiveDate`.

### Startup Synchronization

When the API starts, `NbpExchangeRateStartupSyncService` runs in the background as an `IHostedService`.

Startup flow:

1.  The API waits until the database is reachable.
2.  EF Core migrations are applied automatically.
3.  The service checks the latest locally stored `ExchangeRate` for `RateSource.Code = "NBP"`.
4.  If no NBP rates exist, synchronization starts from `NbpSync:StartDate`, currently `2026-01-01`.
5.  If rates already exist, synchronization starts from the day after the latest stored NBP rate.
6.  The synchronization range ends at `DateTime.Today`.
7.  If the database or external API is unavailable, the API still starts and logs a warning.

This means the first application run fills the database from the configured start date, and later runs only append missing days.

### External Provider Contract

The common contract is:

```csharp
public interface IExternalExchangeRateClient
{
    string SourceCode { get; }
    string SourceName { get; }
    int MaxRangeDays { get; }

    Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
```

For NBP:

*   `SourceCode = "NBP"`
*   `SourceName = "Narodowy Bank Polski"`
*   `MaxRangeDays = 93`
*   API base URL: `https://api.nbp.pl/api/`
*   endpoint pattern: `exchangerates/tables/C/{from}/{to}/?format=json`

The NBP response is mapped to provider-neutral DTOs:

*   `ExternalExchangeRateTableDto`
*   `ExternalExchangeRateItemDto`

This lets future providers reuse the same synchronization service. A new provider should implement `IExternalExchangeRateClient` and map its own response format into the common DTO model.

### Synchronization Logic

`ExchangeRateSyncService` performs the actual import:

1.  Validates the date range.
2.  Finds or creates a `RateSource` for the provider.
3.  Loads existing rates for the provider and selected date range.
4.  Splits the date range into provider-supported chunks.
5.  Calls `IExternalExchangeRateClient.GetBuySellRatesAsync`.
6.  Creates missing `Currency` rows.
7.  Adds new `ExchangeRate` rows.
8.  Skips existing rates for the same currency, source and effective date.
9.  Saves changes to the database.
10. Returns a synchronization result with added/skipped counters, processed ranges and warnings.

For NBP, table `C` provides:

*   `code` -> `Currency.Symbol`
*   `currency` -> `Currency.Name`
*   `bid` -> `ExchangeRate.BuyPrice`
*   `ask` -> `ExchangeRate.SellPrice`
*   `effectiveDate` -> `ExchangeRate.EffectiveDate`

### Backend Endpoints

The frontend uses the following backend endpoints:

```http
GET /api/ExchangeRates/chart?currency=EUR&source=NBP&from=2026-01-01&to=2026-05-18
```

Returns chart points from the local database.

```http
GET /api/ExchangeRates/latest?source=NBP
```

Returns the newest locally stored rate for each currency from the selected source.

```http
POST /api/ExchangeRates/sync/nbp?from=2026-01-01&to=2026-05-18
```

Manually triggers NBP synchronization. This endpoint still contains `nbp` in the route because NBP is currently the only external provider exposed for manual sync. The internal synchronization design is provider-neutral.

### Frontend Display

The Angular route `/rates` displays the exchange rate dashboard without authentication guard for development and testing.

The view:

*   lets the user choose currency, source and date range,
*   loads chart data from `/api/ExchangeRates/chart`,
*   loads the latest table from `/api/ExchangeRates/latest`,
*   can trigger NBP synchronization when local NBP chart data is missing,
*   renders buy and sell prices from backend DTOs.

### Configuration

Relevant configuration values:

```json
{
  "NbpApi": {
    "BaseUrl": "https://api.nbp.pl/api/"
  },
  "NbpSync": {
    "StartDate": "2026-01-01"
  }
}
```

Docker Compose passes the same values through environment variables:

```text
NbpApi__BaseUrl=https://api.nbp.pl/api/
NbpSync__StartDate=2026-01-01
```

### Diagrams

PlantUML diagrams for this module:

*   `UML/ExternalExchangeRateClassDiagram.puml` - class diagram for the external exchange rate integration.
*   `UML/ExternalExchangeRateSequence.puml` - sequence diagram for startup sync, manual sync and frontend read flow.
