# Model danych alertów

## UserAlert

Waluta, typ, strona ceny, kierunek progu, opcjonalne źródło, próg/procent/okres, status, daty, logi i stany ewaluacji.

## UserTradingAlert

Użytkownik, para, typ zdarzenia, kierunek, cena docelowa, opcjonalna ilość minimalna, status, daty i logi.

## AlertLog

Wskazuje dokładnie jeden rodzaj alertu. Przechowuje komunikat, datę, opcjonalną cenę/ilość, identyfikator zdarzenia lub listę źródeł i datę efektywną.

## UserAlertEvaluationState

Unikalna para alert kursowy/źródło i ostatnia oceniona data efektywna.

## Notification

Użytkownik, tekst, data utworzenia, `IsRead`.
