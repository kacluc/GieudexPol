# Baza danych

System używa SQL Server i kontekstu `ApplicationDbContext`. Główne tabele obejmują użytkowników, portfele, kursy, transakcje, zlecenia, wykonania, alerty i powiadomienia.

Ważne ograniczenia:

- unikalny `User.Username`;
- unikalna para `Wallet(UserId, CurrencyId)`;
- unikalna para `TradingPair(BaseCurrencyId, QuoteCurrencyId)`;
- unikalny kurs `ExchangeRate(CurrencyId, RateSourceId, EffectiveDate)`;
- unikalny stan ewaluacji `UserAlertEvaluationState(UserAlertId, RateSourceId)`.

Wolumen Dockera: `gieudexpol-data:/var/opt/mssql`.

Seeder developerski jest idempotentny dla użytkowników, portfeli, źródeł, opłat i kont systemowych. Źródła mockowe utrzymują dane maksymalnie do dnia poprzedzającego bieżącą datę.
