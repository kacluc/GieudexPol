# Alerty

## Dwa rodzaje

1. Alert kursowy `UserAlert`: Threshold, PriceIncrease, PriceDrop dla kursów źródeł.
2. Alert rynku `UserTradingAlert`: poziom Buy, poziom Sell albo `TradeExecution`.

## Praca automatyczna

`AlertMonitoringWorker` cyklicznie tworzy scope i uruchamia oba serwisy ewaluacji. Działa niezależnie od zalogowania. Dodatkowo złożenie zlecenia wywołuje ewaluację alertów danej pary.

## Stany

- `Active`: monitorowany, warunek obecnie niespełniony;
- `Fulfilled`: monitorowany, warunek obecnie spełniony;
- `Inactive`: nie jest oceniany, logi pozostają;
- usunięcie kasuje alert wraz z zależnymi logami.

Nie istnieje mechanizm potwierdzenia/akceptacji alertu. `Fulfilled` nie zatrzymuje workera.

## Wynik

Nowe zdarzenie tworzy `AlertLog` i `Notification`. Powtórzenie tego samego dnia/zdarzenia nie tworzy duplikatu.
