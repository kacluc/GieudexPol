# Model danych kont systemowych

`AccountType` jest enumem przechowywanym na `User`. `Role` pozostaje oddzielnym tekstowym polem autoryzacji.

`RateSource.SystemUserId` jest opcjonalnym kluczem obcym do `User`. Indeks ułatwia znalezienie źródła właściciela konta.

Systemowe portfele używają tego samego `Wallet` co konta regularne. Dzięki temu rezerwacja, dostępne saldo i księgowanie działają wspólnymi metodami.

Brak osobnej tabeli `PlatformTreasury` jest celowy: skarbiec to specjalny `User` rozpoznawany przez `AccountType`.
