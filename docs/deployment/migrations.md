# Migracje EF Core

Migracje znajdują się w `GieudexPol.Infrastructure/Data/Migrations`. API uruchamia `Database.MigrateAsync` w `ExchangeRateStartupSyncService`, a następnie wykonuje seeding.

Przykładowe polecenia:

```powershell
dotnet ef migrations list --project GieudexPol.Infrastructure --startup-project GieudexPol.API
dotnet ef database update --project GieudexPol.Infrastructure --startup-project GieudexPol.API
```

Nie usuwaj istniejących migracji i nie resetuj wolumenu jako standardowej metody aktualizacji. Ostatnie obszary schematu obejmują rynek użytkowników, alerty rynkowe, logi alertów, powiązania transakcji z wykonaniami oraz konta systemowe/skarbca.
