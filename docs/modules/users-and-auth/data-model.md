# Model danych użytkownika

`User` zawiera:

- `Id` i publiczny w tokenie `AuthId`;
- `Username` (e-mail), `DisplayName`, `PasswordHash`;
- `Role`;
- `AccountType`;
- kolekcje portfeli, alertów, logów audytowych, powiadomień, transakcji, zleceń i wymian.

`Username` ma unikalny indeks. `AccountType` ma indeks do filtrowania kont biznesowych.

Relacje transakcji do użytkownika są rozdzielone na `SentTransactions` i `ReceivedTransactions`.
