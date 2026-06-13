# Strategia testów

Testy backendowe obejmują logikę serwisów, repozytoria oraz kontrolery uruchamiane przez `WebApplicationFactory`. EF Core InMemory służy w testach, które nie wymagają zachowania SQL Server.

Testy frontendu sprawdzają komponenty i komunikację HTTP przez Angularowe narzędzia testowe oraz Vitest.

Priorytety:

1. księgowanie i niezmienniki portfela;
2. autoryzacja właściciela zasobu;
3. matching i rezerwacje;
4. wybór kursu, płynność i prowizja;
5. cykl życia alertów;
6. walidacja formularzy.

Polecenia:

```powershell
dotnet test GieudexPol.sln
cd GieudexPol.Frontend
npm test -- --watch=false
```
