# Model danych prowizji

## TransactionFee

- `Id: Guid`
- `Type`
- `FeePercentage`
- `FlatFee`
- `IsActive`

Seeder tworzy aktywne definicje `Transfer`, `Deposit`, `Withdrawal`, `OrderBook`, `InstantExchange` z wartościami 0,5 i 10.

## Historia

`Transaction.AppliedFee` zapisuje faktycznie naliczoną kwotę. `TransactionFeeId` opcjonalnie wskazuje definicję.

`TradeExecution` przechowuje `BuyerFee`, `SellerFee` i `FeeCurrencyId`. `ExchangeExecution` przechowuje `FeeAmount` i `FeeCurrencyId`.

PlatformTreasury jest rekordem `User`, nie osobną tabelą.
