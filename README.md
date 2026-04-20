# 💹 GieudexPol

*wersja "pierwotna do edytkowania" przez pozostałych użytkownikow*

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
| **Frontend** | Angular 17+ | Reaktywny interfejs spekulanta |
| **Backend** | .NET 8 Core | Wydajne WebAPI w architekturze N-Tier |
| **Baza Danych** | MS SQL Server | Relacyjny magazyn danych i historii kursów |
| **AI Agent** | **Cline** (Claude 3.5 Sonnet) | Autonomiczny agent (VS Code) wspomagający rozwój i audyt kodu |
| **Dokumentacja** | PlantUML | Diagramy architektury i przepływu danych |

---

## 📋 Specyfikacja Systemu (Zadania Indywidualne)

### 1. Analiza i wybór agenta AI
W procesie tworzenia systemu wykorzystano agenta **Cline** działającego w środowisku VS Code. 
* **Model:** Claude 3.5 Sonnet (wybrany ze względu na wysoką precyzję w logice finansowej).
* **Metodologia pracy:** **Agentic Loop (Pętla Agencyjna)**. Agent nie tylko generuje kod, ale posiada uprawnienia do zarządzania strukturą plików, uruchamiania terminala (CLI) oraz debugowania błędów kompilacji w czasie rzeczywistym.

### 2. Wybór architektury
System oparty jest na wzorcu **Clean Architecture**, co zapewnia separację logiki biznesowej od infrastruktury:
* **GieudexPol.Domain:** Encje (`Currency`, `Rate`, `Alert`) oraz reguły biznesowe.
* **GieudexPol.Application (BLL):** Interfejsy serwisów, logika obliczeń marżowych i system powiadomień.
* **GieudexPol.Infrastructure (DAL):** Implementacja Entity Framework Core, migracje MS SQL oraz integracja z zewnętrznymi API bankowymi.
* **GieudexPol.API:** Kontrolery REST stanowiące punkt styku dla aplikacji Angular.

### 3. Funkcjonalności (Scope)
Funkcjonalności Użytkownika:
- Rejestracja i logowanie: Bezpieczny dostęp do konta z opcjonalną weryfikacją dwuskładnikową (2FA).

- Portfel cyfrowy: Przejrzysty podgląd salda oraz zarządzanie środkami dostępnymi do handlu.

- Składanie zleceń: Intuicyjny formularz kupna i sprzedaży aktywów po cenie rynkowej.

- Arkusz zleceń (Orderbook): Podgląd wszystkich aktywnych ofert innych użytkowników w czasie rzeczywistym.

- Interaktywne wykresy: Analiza trendów cenowych za pomocą profesjonalnych narzędzi wizualnych.

- Historia transakcji: Pełny wgląd w archiwalne operacje, wpłaty oraz zrealizowane zlecenia.

- Alerty cenowe: System powiadomień informujący o osiągnięciu przez aktywo wyznaczonej ceny.

Funkcjonalności Administratora:
- Zarządzanie użytkownikami: Pełna baza profili z opcją blokowania, usuwania i resetowania haseł.

- Konfiguracja prowizji: Możliwość globalnej zmiany procentowych opłat za każdą transakcję na giełdzie.

- Zarządzanie rynkami: Dodawanie nowych par handlowych (np. BTC/PLN) oraz wstrzymywanie handlu w sytuacjach awaryjnych.

- Monitoring bezpieczeństwa: Podgląd logów systemowych i wykrywanie podejrzanych aktywności lub prób włamań.

- Raporty finansowe: Generowanie zestawień dotyczących wolumenu obrotów i całkowitego zarobku platformy.

---

## 🗄️ Struktura Bazy Danych
Kluczowe encje zarządzane przez Entity Framework:

- Użytkownicy i Bezpieczeństwo:
Users: Przechowuje dane profilowe, skróty haseł (BCrypt/Identity) oraz role (Admin/User).
Wallets: Reprezentuje stan posiadania konkretnej waluty przez użytkownika (np. portfel PLN, portfel BTC).

- Rynek i Kursy:
Currencies: Definicje aktywów (Symbol, Nazwa, Status aktywności).
ExchangeRates: Historia kursów (ceny kupna/sprzedaży) z dokładnością decimal(18,4).

- Operacje i Automatyzacja:
Transactions: Nieusuwalny rejestr wszystkich operacji (kupno/sprzedaż) wraz z naliczoną prowizją.
UserAlerts: Konfiguracja powiadomień – wiąże użytkownika z progiem cenowym danej waluty.
---

## 🚀 Instalacja i Uruchomienie (Dev Environment)

1. **Wymagania:** .NET 8 SDK, Node.js, MS SQL Server.
2. **Backend:**
   ```bash
   dotnet restore
   dotnet ef database update
   dotnet run --project GieudexPol.API