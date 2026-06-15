# Specyfikacja Biznesowa Systemu GieudexPol

## 1. Wprowadzenie
Niniejszy dokument przedstawia wymagania biznesowe dla systemu GieudexPol, przeznaczonego do wymiany walut. Dokument ma na celu określenie funkcjonalności, sposobu pracy oraz rodzajów użytkowników systemu z perspektywy nietechnicznego klienta, stanowiąc załącznik do umowy.

## 2. Kluczowe Funkcjonalności Systemu

### 2.1. Zarządzanie Portfelami
Użytkownicy mogą tworzyć, przeglądać i zarządzać swoimi portfelami walutowymi. Każdy portfel przechowuje informacje o posiadanych walutach i ich ilościach.

### 2.2. Operacje Finansowe
System umożliwia użytkownikom wykonywanie następujących operacji finansowych:
- **Wpłaty i Wypłaty:** Użytkownicy mogą wpłacać środki do swoich portfeli oraz wypłacać je.
- **Szybka Wymiana Walut:** Użytkownicy mogą szybko wymieniać jedną walutę na inną, korzystając z najlepszych dostępnych kursów. System automatycznie wybiera najbardziej korzystne źródło wymiany na podstawie aktualnych danych rynkowych.
- **Zlecenia Limitowe (Rynek Walut):** Użytkownicy mogą składać zlecenia kupna/sprzedaży walut po określonej cenie (cenie limitowej). System będzie realizować te zlecenia, gdy rynek osiągnie zadaną cenę. Obsługiwane są rezerwacje środków, częściowe wykonania zleceń oraz ich anulowanie.

### 2.3. Alerty Kursowe i Rynkowe
Użytkownicy mogą definiować alerty, które powiadomią ich o osiągnięciu określonego kursu waluty lub o innych zdarzeniach rynkowych. Alerty mogą mieć różne stany (aktywny, spełniony, nieaktywny).

### 2.4. Historia Transakcji
System zapewnia dostęp do pełnej historii wszystkich wykonanych transakcji finansowych, umożliwiając łatwe śledzenie operacji.

## 3. Sposób Pracy Systemu

### 3.1. Dostępność i Wydajność
System GieudexPol jest dostępny jako aplikacja internetowa (SPA - Single Page Application) poprzez przeglądarkę internetową. Zapewniona jest wysoka wydajność i stabilność działania, aby użytkownicy mogli sprawnie przeprowadzać operacje finansowe.

### 3.2. Synchronizacja Danych
System w tle automatycznie synchronizuje dane startowe oraz cyklicznie ocenia zdefiniowane alerty kursowe i rynkowe, zapewniając aktualność informacji.

### 3.3. Bezpieczeństwo
System zapewnia odpowiednie mechanizmy bezpieczeństwa, w tym autoryzację opartą na rolach, aby chronić dane użytkowników i ich środki finansowe.

## 4. Rodzaje Użytkowników

### 4.1. Użytkownik Zwykły (Klient)
- **Opis:** Podstawowy użytkownik systemu, który korzysta z funkcjonalności wymiany walut, zarządzania portfelami, składania zleceń i definiowania alertów.
- **Dostęp:** Dostęp do wszystkich funkcji opisanych w sekcji 2, z wyjątkiem funkcji administracyjnych.

### 4.2. Administrator
- **Opis:** Użytkownik z rozszerzonymi uprawnieniami, odpowiedzialny za zarządzanie systemem, monitorowanie operacji i konfigurację.
- **Dostęp:** Pełny dostęp do panelu administracyjnego, umożliwiający m.in. zarządzanie użytkownikami, kontami systemowymi i ustawieniami platformy.

## 5. Prowizje

Wszystkie operacje finansowe podlegają prowizji. Prowizja jest wyliczana centralnie jako `maksymalna z (0,5% kwoty operacji lub równowartość 10 PLN)` i jest księgowana na portfelach konta `PlatformTreasury`.

## 6. Wymagania Niefunkcjonalne (dodatkowo)

- **Użyteczność:** Interfejs użytkownika powinien być intuicyjny i łatwy w obsłudze.
- **Niezawodność:** System powinien działać stabilnie i bezbłędnie.
- **Skalowalność:** System powinien być w stanie obsłużyć rosnącą liczbę użytkowników i transakcji.
- **Bezpieczeństwo:** Ochrona danych osobowych i finansowych użytkowników jest priorytetem.