# Specyfikacja Funkcjonalności Ulubione Waluty

## 1. Cel i Zakres
Funkcjonalność **Ulubione Waluty** umożliwia użytkownikom dodawanie i usuwanie walut do osobistej listy ulubionych. Celem jest ułatwienie monitorowania i szybkiego dostępu do najczęściej używanych walut w aplikacji.

## 2. Funkcjonalności

### 2.1 Dodawanie Waluty do Ulubionych
- Użytkownik może dodać dowolną dostępną walutę do swojej listy ulubionych.
- Waluty są powiązane z kontem użytkownika.

### 2.2 Usuwanie Waluty z Ulubionych
- Użytkownik może usunąć dowolną walutę z listy ulubionych.

## 3. Model Danych

### 3.1 Entytety

#### Entyteta `FavouriteCurrency`
- **Id**: int (unikalny identyfikator)
- **UserId**: int (referencja do użytkownika)
- **CurrencyCode**: string (kod waluty, np. "EUR", "USD")
- **CreatedAt**: DateTime (data dodania do ulubionych)

## 4. Endpointy API

### 4.1 Dodawanie Ulubionej Waluty
- **Endpoint:** `POST /api/favorites`
- **Opis:** Dodaje wybraną walutę do listy ulubionych użytkownika.
- **Payload (`AddFavoriteCurrencyDto`):**
  ```json
  {
    "CurrencyCode": "string" // Kod waluty, np. "EUR", "USD"
  }
  ```
- **Odpowiedzi:**
  - **Succes:** `201 Created`
  - **Błąd:** `400 Bad Request` (jeśli waluta już istnieje w ulubionych)
  - **Błąd:** `404 Not Found` (jeśli waluta nie istnieje w systemie)

### 4.2 Pobieranie Ulubionych Walut
- **Endpoint:** `GET /api/favorites`
- **Opis:** Pobiera listę ulubionych walut dla zautoryzowanego użytkownika.
- **Odpowiedź (`List<FavoriteCurrencyDto>`):**
  ```json
  [
    {
      "CurrencyCode": "string",
      "CurrencyName": "string"
    },
    ...
  ]
  ```

### 4.3 Usuwanie Ulubionej Waluty
- **Endpoint:** `DELETE /api/favorites/{currencyCode}`
- **Opis:** Usuwa wybraną walutę z listy ulubionych użytkownika.
- **Parametry:** `currencyCode` (kod waluty do usunięcia)
- **Odpowiedzi:**
  - **Succes:** `200 OK`
  - **Błąd:** `404 Not Found` (jeśli waluta nie istnieje w ulubionych)

## 5. DTOs

### 5.1 `AddFavoriteCurrencyDto`
```csharp
public class AddFavoriteCurrencyDto
{
    public string CurrencyCode { get; set; }
}
```

### 5.2 `FavoriteCurrencyDto`
```csharp
public class FavoriteCurrencyDto
{
    public string CurrencyCode { get; set; }
    public string CurrencyName { get; set; }
}
```

## 6. Przepływy Biznesowe

### 6.1 Dodawanie Waluty do Ulubionych
1. Użytkownik wysyła żądanie `POST /api/favorites` z kodem waluty.
2. Backend sprawdza, czy waluta istnieje w systemie.
3. Jeśli waluta istnieje, backend sprawdza, czy nie jest już w ulubionych użytkownika.
4. Jeśli waluta nie jest już w ulubionych, dodaje ją do listy ulubionych i zwraca odpowiedź `201 Created`.
5. Jeśli waluta już jest w ulubionych, zwraca błąd `400 Bad Request`.

### 6.2 Usuwanie Waluty z Ulubionych
1. Użytkownik wysyła żądanie `DELETE /api/favorites/{currencyCode}`.
2. Backend sprawdza, czy waluta istnieje w ulubionych użytkownika.
3. Jeśli waluta istnieje, usuwa ją z listy ulubionych i zwraca odpowiedź `200 OK`.
4. Jeśli waluta nie istnieje w ulubionych, zwraca błąd `404 Not Found`.

## 7. Diagramy UML
- **Diagram Klas:** `UML/FavoriteCurrencyClassDiagram.puml`
- **Diagram Sekwencji:** `UML/FavoriteCurrencySequence.puml`

## 8. Przykłady Użycia

### Przykład 1: Dodawanie Waluty do Ulubionych
```http
POST /api/favorites
Content-Type: application/json

{
  "CurrencyCode": "EUR"
}
```
**Odpowiedź:**
```http
HTTP/1.1 201 Created
```

### Przykład 2: Usuwanie Waluty z Ulubionych
```http
DELETE /api/favorites/EUR
```
**Odpowiedź:**
```http
HTTP/1.1 200 OK
```

## 9. Zależności i Integracje
- **Backend:** Używa `FavoriteCurrencyService` i `FavoriteCurrencyRepository` do zarządzania ulubionymi walutami.
- **Frontend:** Wykorzystuje usługi `FavoriteCurrencyService` do interakcji z API.

## 10. Testowanie
- **Testy jednostkowe:** `FavoriteCurrencyServiceTests.cs`