# Reguły użytkowników i auth

- E-mail jest loginem i musi być unikalny.
- Rejestracja wymaga nazwy 2–50 znaków, e-maila, hasła min. 6 znaków i potwierdzenia.
- Hasła są przechowywane jako hash `PasswordHasher`.
- JWT zawiera `NameIdentifier = AuthId`, e-mail i rolę.
- Domyślny czas ważności tokenu to 60 minut, jeżeli konfiguracja nie poda innej wartości.
- Zwykła rejestracja tworzy konto regularne.
- `RateSourceSystem` i `PlatformTreasury` nie mogą się zalogować.
- Endpointy właścicielskie powinny identyfikować użytkownika z JWT, nie ufać `UserId` z body.

## Uwaga

Role `Admin` i `SuperAdmin` istnieją, lecz większość kontrolerów admina deklaruje wyłącznie `Roles = Admin`. To wymaga uwzględnienia przy przyszłej polityce SuperAdmin.
