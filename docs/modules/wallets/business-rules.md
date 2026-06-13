# Reguły portfeli

- `AvailableBalance = Balance - ReservedBalance`.
- Kwoty operacji muszą być dodatnie.
- Nie można obciążyć więcej niż dostępne saldo.
- Para `(UserId, CurrencyId)` jest unikalna.
- Wpłata wymaga `amount > fee`.
- Wypłata wymaga `AvailableBalance >= amount + fee`.
- Transfer wymaga `AvailableBalance >= amount + fee`.
- Nie można transferować do siebie.
- Nie można wykonać zwykłego transferu do `RateSourceSystem` ani `PlatformTreasury`.
- Rezerwacja może dotyczyć tylko środków dostępnych.
- Anulowanie zlecenia zwalnia wyłącznie rezerwację pozostałej części.

## Bezpieczeństwo

Trasy zawierające `userId` są starszym kontraktem, lecz kontrolery porównują go z użytkownikiem znalezionym przez claim `NameIdentifier`.

## Uwaga

Ogólny endpoint `PUT /api/Wallets/{id}` istnieje i chroni właściciela, ale przyjmuje encję `Wallet`; do operacji finansowych należy używać dedykowanych endpointów.
