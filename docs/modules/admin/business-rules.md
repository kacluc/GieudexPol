# Reguły administracji

- Kontrolery admina wymagają obecnie roli `Admin`.
- Lista zwykłych użytkowników nie zawiera kont systemowych.
- Nie można przez panel zmienić roli ani hasła konta systemowego.
- Panel testowych kursów może modyfikować wyłącznie chronione kody developerskie.
- Ręczna ewaluacja alertów korzysta z tego samego `AlertEvaluationService` co worker.
- Zlecenie źródła wymaga aktywnego źródła, konta typu `RateSourceSystem`, aktywnej pary i dostępnych środków.
- Podgląd kont systemowych jest tylko do odczytu.

## Uwagi / do weryfikacji

`SuperAdminUser` istnieje w `AccountType`, lecz bieżące atrybuty admina nie wymieniają roli `SuperAdmin`.
