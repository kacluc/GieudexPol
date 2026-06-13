# Reguły prowizji

`fee = max(0,5% kwoty, równowartość 10 PLN)`

- Dla PLN minimum wynosi 10 PLN.
- Dla innej waluty kalkulator pobiera kurs tej waluty do PLN.
- Preferowany jest `MidPrice`; fallback to średnia Buy/Sell.
- Minimum w walucie obcej to `10 PLN / rateToPln`.
- Wynik jest zaokrąglany do 4 miejsc, `AwayFromZero`.
- Brak waluty lub kursu do PLN jest błędem.

## Księgowanie

- Deposit: użytkownik dostaje `amount - fee`.
- Withdrawal: użytkownik traci `amount + fee`.
- Transfer: odbiorca dostaje `amount`, nadawca traci `amount + fee`.
- InstantExchange: fee w walucie źródłowej, obciążenie `amount + fee`.
- OrderBook: kupujący i sprzedający płacą fee w walucie kwotowanej.

Repozytorium portfela księguje fee operacji podstawowych do PlatformTreasury; serwisy wymiany i matchingu robią to w ramach własnych transakcji.
