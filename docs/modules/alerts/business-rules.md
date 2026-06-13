# Reguły alertów

## Kursowe

- `UserBuysCurrency` używa `SellPrice`.
- `UserSellsCurrency` używa `BuyPrice`.
- `MidPrice` używa `MidPrice` lub średniej Buy/Sell.
- Threshold wymaga wartości i kierunku.
- Wzrost/spadek wymaga dodatniego procentu.
- `RateSourceId = null` oznacza wszystkie aktywne źródła.
- `TimeFrameHours` puste lub 24 oznacza poprzedni dostępny kurs dzienny.
- Stan ewaluacji zapamiętuje ostatnią datę dla alertu i źródła.
- Zwykły użytkownik nie może wybrać źródła mockowego; dane mock są filtrowane także z jego logów.

## Rynkowe

- `SellOrder`: użytkownik chce kupić, więc obserwuje najtańszą sprzedaż i kierunek `<=`.
- `BuyOrder`: użytkownik chce sprzedać, więc obserwuje najlepsze kupno i kierunek `>=`.
- `TradeExecution` używa kierunku zapisanego w alercie.
- Opcjonalne `MinimumAmount` filtruje zagregowany poziom lub wykonanie.
- Własne zlecenia użytkownika nie spełniają jego alertu poziomu.

`Fulfilled` wraca do `Active`, gdy bieżący warunek przestaje być spełniony.
