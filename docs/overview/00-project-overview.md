# Przegląd projektu

GieudexPol jest systemem walutowym dla użytkowników aplikacji. Dane kursowe pochodzą z wielu źródeł, natomiast rynek zleceń jest wewnętrznym rynkiem użytkowników i kont systemowych, a nie kopią rynku Forex.

## Funkcje

- rejestracja, logowanie i JWT;
- portfele wielowalutowe z saldem zarezerwowanym;
- wpłaty, wypłaty, transfery i historia;
- szybka wymiana z automatycznym wyborem płynnego źródła;
- niewykonujący transakcji symulator wymiany;
- limitowy rynek walut z matchingiem;
- alerty kursowe i rynkowe oraz powiadomienia;
- źródła rzeczywiste i dwa źródła testowe;
- panel użytkowników, testowych kursów i kont systemowych.

## Granice

`ExchangeRate` opisuje kurs źródła. `Order` opisuje ofertę użytkownika lub konta systemowego. Te dane nie są tym samym i kursy nie są automatycznie zamieniane w poziomy order booka.

## Stan częściowy

W repozytorium są starsze, ogólne endpointy CRUD. Nie wszystkie są ograniczone rolą administratora. SuperAdmin jest typem konta i rolą modelową, ale nie ma osobnego panelu ani polityki autoryzacyjnej.
