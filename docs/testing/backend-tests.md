# Testy backendu

Zestaw `GieudexPol.Tests` zawiera m.in.:

- `WalletServiceTests`, `WalletsControllerTests`;
- `TransactionServiceTests`, `PlatformTreasuryBookingTests`;
- `InstantExchangeServiceTests`;
- `OrderBookServiceTests`;
- `AlertEvaluationServiceTests`, `TradingAlertEvaluationServiceTests`;
- testy kontrolerów alertów i admina;
- `DevelopmentDataSeederTests`;
- testy filtrowania rankingu i kont systemowych;
- testy klientów oraz synchronizacji kursów.

Symulator wymiany jest sprawdzany pod kątem najlepszego płynnego źródła, limitu 7 dni, walidacji oraz braku zapisów do sald, `Transaction` i `ExchangeExecution`.

Testy order booka obejmują kolejność price-time, częściowe wykonania, prowizje, rezerwacje, anulowanie i bezpieczeństwo właściciela.
