# Model danych rynku

## TradingPair

Waluta bazowa, kwotowana, `IsActive`, `TickSize`. Para walut jest unikalna.

## Order

Użytkownik, para, `Side`, `Type`, `Status`, cena, ilość pierwotna i pozostała, wykonana wartość kwotowana, zapłacone fee, daty utworzenia/zamknięcia.

## TradeExecution

Zlecenie Buy i Sell, para, cena, ilość, fee kupującego i sprzedającego, waluta fee, czas oraz wpisy historii.

## Indeksy

- para bazowa/kwotowana: unikalna;
- aktywne wyszukiwanie zleceń: para, status, strona, cena, czas;
- osobne indeksy kluczy zleceń wykonania.
