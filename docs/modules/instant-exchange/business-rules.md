# Reguły szybkiej wymiany

- Kwota musi być dodatnia, a waluty różne.
- Konto użytkownika nie może być kontem źródła ani PlatformTreasury.
- Kurs musi mieć `EffectiveDate` od dzisiaj do 7 dni wstecz.
- Źródło i jego konto systemowe muszą być aktywne/skonfigurowane.
- Dla zwykłego użytkownika odrzucane są oba źródła mockowe.
- Dla waluty wydawanej używany jest `BuyPrice`, a dla otrzymywanej `SellPrice`.
- Wynik: `amountFrom * fromRateToPln / toRateToPln`.
- Kandydat musi posiadać `AvailableBalance` waluty docelowej co najmniej równy wynikowi.
- Wybierany jest najwyższy wynik, potem najnowsza data i kod źródła.
- Fee jest naliczane w walucie wydawanej; użytkownik płaci `amountFrom + fee`.
- Preview nie wymaga posiadania środków, lecz zwraca `HasSufficientFunds`.

Brak kursu w oknie 7 dni i brak płynnego źródła są osobnymi błędami.
