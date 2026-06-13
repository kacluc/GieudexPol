# Model danych szybkiej wymiany

## ExchangeExecution

- użytkownik i źródło wykonania;
- waluta źródłowa i docelowa;
- kwota wejściowa i wyjściowa;
- zastosowany kurs;
- kwota i waluta prowizji;
- czas wykonania;
- kolekcja wpisów `Transaction`.

## Historia

Wykonanie tworzy:

1. `InstantExchangeSell`: użytkownik przekazuje kwotę źródłu, wpis zawiera fee;
2. `InstantExchangeBuy`: źródło przekazuje walutę docelową użytkownikowi.

Oba wpisy wskazują ten sam `ExchangeExecution`.

Preview korzysta tylko z DTO i nie ma osobnej encji.
