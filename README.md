# 💹 GieudexPol

Adres strony:
https://gieudexpol2-production.up.railway.app

*wersja "pierwotna do edytowania" przez pozostałych użytkownikow*

> **Kryptonim projektowy:** `Waluty robią brrrrrrr`  
> **Status:** W fazie implementacji (v1.0)  
> **Cel:** Maksymalizacja zysku z wahań rynkowych poprzez zaawansowaną analitykę danych Live.

---


## 💡 Idea systemu
GieudexPol to silnik, który automatycznie pobiera kursy walut z różnych źródeł (np. NBP, API komercyjne), nakłada na nie własną marżę i szuka różnic w cenach (arbitrażu). System działa jak inteligentny filtr: zamiast przeglądać dziesiątki tabel, użytkownik dostaje gotowe zestawienie par walutowych, na których w danej sekundzie można zarobić.

---


## 🎯 Cel projektu
Agregacja danych: Automatyczne ściąganie kursów z wielu API bez konieczności ręcznego sprawdzania stron banków.

Silnik marżowy: Stworzenie modułu, który w czasie rzeczywistym dolicza prowizje do kursów bazowych.

Detekcja anomalii: Zaprogramowanie algorytmu, który wyłapuje błędy cenowe lub nagłe skoki kursów.

Szybkość powiadomień: Skrócenie czasu od wystąpienia okazji rynkowej do poinformowania o tym użytkownika.


---


## 🛠️ Stack Technologiczny
| Warstwa | Technologia | Opis |
| :--- | :--- | :--- |
| **Frontend** | Angular 21 | Reaktywny interfejs spekulanta |
| **Backend** | .NET 10 | Wydajne WebAPI w architekturze N-Tier |
| **Baza Danych** | MS SQL Server | Relacyjny magazyn danych i historii kursów |
| **AI Agent** | **Cline** (Gemini-2.5-flash) | Autonomiczny agent (VS Code) wspomagający rozwój i audyt kodu |
| **Dokumentacja** | PlantUML | Diagramy architektury i przepływu danych |

---


## 📋 Specyfikacja Systemu (Zadania Indywidualne)


### 1. Analiza i wybór agenta AI
W procesie tworzenia systemu wykorzystano agenta **Cline** działającego w środowisku VS Code. 
* **Model:** Gemini-2.5-flash (wybrany ze względu na wysoką precyzję w logice finansowej).
* **Metodologia pracy:** **Agentic Loop (Pętla Agencyjna)**. Agent nie tylko generuje kod, ale posiada uprawnienia do zarządzania strukturą plików, uruchamiania terminala (CLI) oraz debugowania błędów kompilacji w czasie rzeczywistym.


### 2. Wybór architektury
System oparty jest na wzorcu **Clean Architecture**, co zapewnia separację logiki biznesowej od infrastruktury:
* **GieudexPol.Domain:** Encje (`Currency`, `Rate`, `Alert`) oraz reguły biznesowe.
* **GieudexPol.Application (BLL):** Interfejsy serwisów, logika obliczeń marżowych i system powiadomień.
* **GieudexPol.Infrastructure (DAL):** Implementacja Entity Framework Core, migracje MS SQL oraz integracja z zewnętrznymi API bankowymi.
* **GieudexPol.API:** Kontrolery REST stanowiące punkt styku dla aplikacji Angular.


### 📂 Directory Structure:
*   `GieudexPol.Domain`: Core entities (`Currency`, `Rate`, `Alert`) and business rules.
*   `GieudexPol.Application`: Business services (BLL), calculation logic, and notification interfaces.
*   `GieudexPol.Infrastructure`: Data Access Layer (DAL) implementation using Entity Framework Core; handles external API integration (e.g., bank APIs).
*   `GieudexPol.API`: REST Controllers and Middleware (JWT, CORS) serving as the primary entry point for the Angular frontend.


### 3. Funkcjonalności (Scope)
Funkcjonalności Użytkownika:
- **Uwierzytelnianie (Logowanie i Rejestracja):** Zaimplementowano kompletny mechanizm rejestracji i logowania z walidacją danych po stronie serwera i klienta. Domyślna strona startowa aplikacji to ekran logowania, a dostęp do głównych funkcjonalności wymaga autoryzacji.

- Portfel cyfrowy: Przejrzysty podgląd salda oraz zarządzanie środkami dostępnymi do handlu.

- Składanie zleceń: Intuicyjny formularz kupna i sprzedaży aktywów po cenie rynkowej.

- Arkusz zleceń: Podgląd aktywnych ofert kupna i sprzedaży składanych przez użytkowników.

- Interaktywne wykresy: Analiza trendów cenowych za pomocą profesjonalnych narzędzi wizualnych.

- Historia transakcji: Pełny wgląd w archiwalne operacje, wpłaty oraz zrealizowane zlecenia.

- Alerty cenowe: System powiadomień informujący o osiągnięciu przez aktywo wyznaczonej ceny.

**NOWO:** Zarządzanie Portfelem (Wallet Management): Centralizowany moduł dla podglądu i realizacji transakcji wymiany walut. Wykorzystuje endpoint `POST /api/wallet/trade` do bezpiecznego transferu środków między walutami użytkownika, zarządzając stanem salda w rekordzie `Wallets`.

Funkcjonalności Administratora:
- **User Management:** Pełna baza profili z opcją blokowania, usuwania i resetowania haseł.

- Konfiguracja prowizji: Możliwość globalnej zmiany procentowych opłat za każdą transakcję na giełdzie.

- Zarządzanie rynkami: Dodawanie nowych par handlowych (np. BTC/PLN) oraz wstrzymywanie handlu w sytuacjach awaryjnych.

- Monitoring bezpieczeństwa: Podgląd logów systemowych i wykrywanie podejrzanych aktywności lub prób włamań.

- Raporty finansowe: Generowanie zestawień dotyczących wolumenu obrotów i całkowitego zarobku platformy.

---
## ⚙️ Deployment & Setup Instructions (DevOps)
Szczegółowy proces wdrożenia, konfiguracja środowiska lokalnego oraz instrukcje techniczne są opisane w dedykowanym pliku: **`DEPLOYMENT_GUIDE.md`**. Prosimy o jego zapoznanie się przed rozpoczęciem pracy z systemem.
