# Rynek walut

## Cel

Moduł przechowuje limitowe zlecenia Buy/Sell użytkowników i kont źródeł, agreguje publiczny arkusz oraz dopasowuje zlecenia według ceny i czasu.

## Pojęcia

Dla EUR/PLN:

- Buy: użytkownik chce kupić EUR za PLN;
- Sell: użytkownik chce sprzedać EUR za PLN.

## Serwisy i encje

`TradingPair`, `Order`, `TradeExecution`, `OrderBookService`, `OrderMatchingService`.

## Scenariusze

- złożenie i rezerwacja;
- natychmiastowa próba matchingu;
- pełne lub częściowe wykonanie;
- pozostawienie reszty w arkuszu;
- anulowanie i zwolnienie reszty rezerwacji;
- adminowe zlecenie w imieniu konta źródła.

Publiczny arkusz pokazuje wyłącznie zagregowane poziomy, bez właścicieli.
