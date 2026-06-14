# Specyfikacja Symulatora Wymiany Walut

## 1. Wstęp
Symulator wymiany walut jest narzędziem, które pozwala użytkownikom na symulację wymiany walutowych transakcji bez rzeczywistego wpływania na ich portfele.

## 2. Cel
- Pozwolenie użytkownikom na testowanie różnych scenariuszy wymiany walut.
- Wyświetlenie przewidywanych wyników transakcji na podstawie aktualnych kursów walutowych.
- Ułatwienie zrozumienia, jak działają kursy wymiany walut.

## 3. Funkcjonalności

### 3.1. Wymiana Walut
- **Wymagania:**
  - Użytkownik podaje kwotę waluty źródłowej (np. PLN).
  - Użytkownik wybiera walutę docelową (np. USD, EUR).
  - System pobiera aktualny kurs wymiany walutowej.
  - System oblicza i wyświetla przewidywane wyniki wymiany.

- **Przykład:**
  - Użytkownik wprowadza 1000 PLN i wybiera USD.
  - System wyświetla wynik: 250 USD przy aktualnym kursie 4.00 PLN/USD.

### 3.2. Wyświetlanie Kursów Walutowych
- **Wymagania:**
  - System pobiera aktualne kursy walutowe z zewnętrznych źródeł (np. NBP, ECB).
  - Kursy są wyświetlane w interfejsie użytkownika.


### 3.3. Walidacja Danych
- **Wymagania:**
  - System sprawdza, czy wprowadzona kwota jest dodatnia.
  - System sprawdza, czy wybrane waluty są dostępne w systemie.
  - System informuje użytkownika o błędach wejściowych.

## 4. Interfejs Użytkownika
- **Strona główna symulatora:**
  - Pole do wprowadzenia kwoty waluty źródłowej.
  - Wybór waluty źródłowej z listy dostępnych walut.
  - Wybór waluty docelowej z listy dostępnych walut.
  - Przycisk "Symuluj wymianę".
  - Wyświetlenie wyniku symulacji.


## 5. Techniczne Wymagania

### 5.1. Zależności
- **Zewnętrzne API:**
  - NBP API dla kursów walutowych.


- **Baza danych:**
  - Rejestracja symulowanych transakcji.

### 5.2. API Endpoints
- **POST /api/simulate-exchange**
  - Przyjmuje żądanie symulacji wymiany walut.
  - Zwraca wynik symulacji.

- **GET /api/exchange-rates**
  - Zwraca aktualne kursy walutowe.



## 6. Przykładowy Scenariusz Użytkownika

### Symulacja Wymiany Walut
1. Użytkownik otwiera symulator wymiany walut.
2. Wprowadza kwotę 1000 PLN.
3. Wybiera walutę docelową USD.
4. Kliknie przycisk "Symuluj wymianę".
5. System wyświetla wynik: 250 USD przy kursie 4.00 PLN/USD.


## 7. Błędy i Obsługa Wyjątków
- **Brak dostępności kursów walutowych:**
  - System informuje użytkownika o braku dostępności kursów i prosi o odświeżenie strony.

- **Nieprawidłowe dane wejściowe:**
  - System wyświetla komunikat o błędzie, jeśli wprowadzona kwota jest ujemna lub nie jest liczbą.

## 8. Testowanie
- **Testy jednostkowe:**
  - Sprawdzenie poprawności obliczeń kursów wymiany.

- **Testy integracyjne:**
  - Sprawdzenie interakcji z zewnętrznymi API kursów walutowych.
  
## 9. Dokumentacja
- **Dokumentacja API:**
  - Opis wszystkich dostępnych endpointów.
  - Przykłady żądań i odpowiedzi.

- **Dokumentacja użytkownika:**
  - Instrukcje obsługi symulatora.
  - Wyjaśnienie funkcjonalności i korzyści z użycia symulatora.