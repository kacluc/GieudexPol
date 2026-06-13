# Reguły rynku walut

- Obsługiwany jest tylko `OrderType.Limit`.
- Cena i ilość muszą być dodatnie; ilość ma maksymalnie 4 miejsca.
- Cena jest wielokrotnością `TickSize`.
- Buy rezerwuje walutę kwotowaną: `amount * limitPrice + przewidywane fee`.
- Sell rezerwuje ilość waluty bazowej.
- Buy dopasowuje Sell z `Sell.Price <= Buy.Price`: najniższa cena, potem najstarsze.
- Sell dopasowuje Buy z `Buy.Price >= Sell.Price`: najwyższa cena, potem najstarsze.
- Cena wykonania jest ceną zlecenia oczekującego.
- Zlecenia tego samego konta nie są dopasowywane.
- Prowizje obu stron są liczone narastająco i trafiają do PlatformTreasury w walucie kwotowanej.
- Sprzedający otrzymuje wartość wykonania pomniejszoną o swoje fee.
- Po częściowym wykonaniu status to `PartiallyFilled`.
- Anulować można tylko własne `Open` lub `PartiallyFilled`.
- Głębokość arkusza jest ograniczana do 1–100.

System nie obsługuje market orders, stop loss, margin ani SignalR.
