# Instrukcje dla Agenta (Projekt GieudexPol)

Jesteś agentem pracującym w środowisku z rygorystycznym limitem kontekstu (32k tokenów). Musisz zarządzać pamięcią operacyjną poprzez ekstremalną dyscyplinę w czytaniu plików oraz ścisłe trzymanie się wyznaczonych wersji technologicznych.

## 1. STANDARDY TECHNOLOGICZNE (BEZWZGLĘDNE)
Wszystkie generowane i modyfikowane pliki muszą być zgodne z poniższym stosem technologicznym. KATEGORYCZNY ZAKAZ mieszania wersji lub używania przestarzałych wzorców (vibecoding).
- **Backend: .NET 10 (C# 14)** -> Używaj wyłącznie nowoczesnej składni (Minimal APIs, Primary Constructors, Required Properties, Collection Expressions).
- **Frontend: Angular 21** -> Kod musi być w 100% **Zoneless** (bez Zone.js). Używaj wyłącznie **Signals** (`signal()`, `computed()`, `effect()`), nowego bloku kontrolnego (`@if`, `@for`) oraz **Signal Forms**. Zakaz używania starych modułów NgModules (wszystko ma być `standalone: true`).

## 2. ZASADY SKANOWANIA I CZYTANIA
- **ZAKAZ używania `list_files` na głównym katalogu.** Jeśli musisz poznać strukturę, skanuj tylko konkretny podfolder (np. `src/app/services`).
- **ZAKAZ czytania plików w ciemno.** Zanim użyjesz `read_file`, musisz uzasadnić użytkownikowi, dlaczego ten plik jest niezbędny.
- **ZAKAZ czytania plików binarnych, logów oraz folderów `bin/`, `obj/`, `node_modules/`, `.git/`.**

## 3. INSTRUKCJE EDYCJI (UNIKANIE BŁĘDÓW STRUKTURALNYCH)
Aby uniknąć błędów typu `Could not find oldString` oraz problemów z końcami linii (CRLF/LF):
- **Małe pliki (do 100 linii):** Zamiast cząstkowej edycji, przygotuj pełną zawartość pliku i nadpisz go w całości.
- **Duże pliki:** Twój `oldString` musi być krótki (maksymalnie 3-5 linii kodu). Nie dołączaj do niego długich bloków komentarzy ani pustych linii, które mogą różnić się znakami formatowania na dysku użytkownika.
- **Formatowanie:** Generuj kod używając standardowych wcięć (spacje, nie tabulatory).

## 4. WORKFLOW: DZIEL I ZWYCIĘŻAJ (Z UWZGLĘDNIENIEM DOCKERA)
Każde zadanie realizuj w cyklu:
1. **Analiza Listy:** Poproś użytkownika o listę zmienionych plików (np. z `git status` lub `git diff`).
2. **Czytanie Punktowe:** Czytaj tylko JEDEN plik na raz. Wyciągnij z niego najważniejsze informacje i zapisz je w pamięci roboczej.
3. **Potwierdzenie:** Przed wprowadzeniem zmian przedstaw plan modyfikacji w punktach i poczekaj na akceptację użytkownika.
4. **Implementacja i Docker:** Wprowadzaj zmiany partiami. Po każdej partii zmian upewnij się, że kod nie zepsuje budowania kontenerów w Dockerze. Nie otwieraj i nie edytuj 5 plików naraz.

## 5. ARCHITEKTURA I HIGIENA KODU
- **Backend (.NET):** Interesują nas tylko sygnatury metod w Kontrolerach/Minimal APIs i definicje w DTO. Ignoruj logikę wewnętrzną zewnętrznych serwisów, jeśli nie wpływa na kontrakt API.
- **Frontend (Angular):** 
  - Najpierw twórz/aktualizuj modele (`.dto.ts` lub `.model.ts`).
  - Potem aktualizuj serwisy komunikacji z API (`.service.ts`).
  - Na końcu komponenty i szablony HTML.

## 6. HIGIENA KONTEKSTU I REAKCJA NA BŁĘDY
- Jeśli historia rozmowy staje się długa, podsumuj postępy i poproś użytkownika o rozpoczęcie Nowego Zadania (New Task), aby wyczyścić RAM.
- Po zakończeniu edycji pliku, "zapomnij" o jego szczegółach, zostawiając w pamięci tylko ogólny wniosek (kontrakt).
- Jeśli otrzymasz błąd "exceeds context size", natychmiast przestań używać narzędzi odczytu i poproś użytkownika o ręczne wklejenie fragmentu kodu.

## 7. ZASADA DRY (DON'T REPEAT YOURSELF) I AUDYT ISTNIEJĄCYCH ROZWIĄZAŃ
**PRZED DODANIEM NOWYCH ZALEŻNOŚCI LUB FRAMEWORKÓW:**
1. **Sprawdź package.json**: Zidentyfikuj już zainstalowane biblioteki i frameworki.
2. **Przeanalizuj pliki konfiguracyjne**: Sprawdź, czy projekt już używa alternatywnych rozwiązań (np. TailwindCSS zamiast Bootstrap).
3. **Zapytaj użytkownika**: Jeśli nie jesteś pewien, czy dany framework/biblioteka jest już używana, zapytaj przed dodaniem nowej zależności.
4. **Preferuj istniejące rozwiązania**: Jeśli projekt już używa frameworka CSS (np. TailwindCSS), użyj go zamiast dodawać nowy (np. Bootstrap).

**PRZYKŁAD Z PROJEKTU:**
- Projekt używa TailwindCSS (widoczny w package.json i styles.css)
- Nie dodawaj Bootstrap, jeśli Tailwind może rozwiązać problem
- Zawsze sprawdzaj, czy nie ma już zaimplementowanej alternatywy

**KONSEKWENCJE NIEPRZESTRZEGANIA:**
- Zduplikowane zależności zwiększają rozmiar bundle
- Konflikty między frameworkami CSS/JS
- Niepotrzebne zużycie zasobów
- Trudności w utrzymaniu kodu
