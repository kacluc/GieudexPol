# Administracja

## Dostępne funkcje

- lista, tworzenie, zmiana roli i reset hasła użytkowników;
- zarządzanie kursami `MOCK_BANK_A` i `MOCK_BANK_B`;
- ręczna ewaluacja alertów kursowych;
- podgląd kont systemowych i ich portfeli;
- składanie zleceń jako konto aktywnego źródła kursów.

Frontend ma trasy:

- `/admin/users`;
- `/admin/test-exchange-rates`;
- `/admin/system-accounts`.

## Ograniczenia

Nie ma panelu konfiguracji prowizji, raportów finansowych, logów bezpieczeństwa ani osobnego panelu SuperAdmin. Admin nie loguje się jako konto źródła; działa przez dedykowany endpoint.

## Co pokazać

Zmienić kurs mockowy, ręcznie ocenić alert, sprawdzić portfel konta źródła i wystawić zlecenie źródła.
