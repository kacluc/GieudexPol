# API użytkowników i auth

## Rejestracja

`POST /api/Auth/register`

```json
{
  "displayName": "Jan Kowalski",
  "email": "jan@example.com",
  "password": "Haslo123!",
  "confirmPassword": "Haslo123!"
}
```

## Logowanie

`POST /api/Auth/login`

```json
{
  "email": "jan@example.com",
  "password": "Haslo123!"
}
```

Odpowiedź zawiera token i dane potrzebne klientowi.

## Profil techniczny

`GET /api/Users/{username}` wymaga JWT.

Błędy walidacji modelu zwracają `400`; błędne dane logowania są obsługiwane przez komendę logowania.
