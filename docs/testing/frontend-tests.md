# Testy frontendu

Frontend używa `ng test`, a wykonawcą testów jest Vitest. Testy obejmują m.in. logowanie, rejestrację, alerty, rynek walut, portfel i symulator wymiany.

Dla symulatora sprawdzane są:

- brak requestu dla kwoty niedodatniej;
- brak requestu dla jednakowych walut;
- prezentacja poprawnej odpowiedzi;
- prezentacja błędu;
- brak wywołania endpointu realnej wymiany.

Build produkcyjny:

```powershell
npm run build
```
