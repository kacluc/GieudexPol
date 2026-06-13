# Użytkownicy i autoryzacja

## Cel

Moduł rejestruje konta, weryfikuje hasła i wydaje JWT. Pozostałe kontrolery wiążą zasoby z użytkownikiem znalezionym przez `AuthId`.

## Elementy

- `AuthController`;
- komendy MediatR logowania i rejestracji;
- `IdentityService`, `JwtService`;
- `UserRepository`;
- guardy i interceptor JWT w Angularze.

## Role a typ konta

`Role` (`User`, `Admin`, `SuperAdmin`) trafia do JWT i służy `[Authorize]`. `AccountType` służy logice biznesowej i może oznaczać także konto systemowe lub skarbiec.

Konta systemowe nie przechodzą weryfikacji hasła.
