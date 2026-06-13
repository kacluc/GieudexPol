# Reguły kursów walut

- `BuyPrice`: źródło kupuje walutę od użytkownika.
- `SellPrice`: źródło sprzedaje walutę użytkownikowi.
- `MidPrice`: kurs średni; gdy nie istnieje, część modułów używa średniej `BuyPrice` i `SellPrice`.
- Unikalność: waluta, źródło, data efektywna.
- Źródło musi być aktywne, aby uczestniczyć w alertach i szybkiej wymianie.
- Szybka wymiana przyjmuje wyłącznie kursy od dzisiaj do 7 dni wstecz.
- Dla par krzyżowych źródło musi mieć najnowszy kurs obu walut względem PLN.
- Zwykły użytkownik nie dostaje źródeł mockowych w liście źródeł alertu ani w szybkiej wymianie.
- Admin może tworzyć, edytować i usuwać tylko kursy źródeł developerskich przez dedykowany panel.

## Przypadki brzegowe

- Brak publikacji w weekend nie jest sztucznie uzupełniany.
- Brak kursu PLN w źródle referencyjnym może uniemożliwić normalizację dnia.
- Kursy starsze niż 7 dni mogą być widoczne historycznie, ale nie kwalifikują się do szybkiej wymiany.
