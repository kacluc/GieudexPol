# Rozwój lokalny

## Wymagania

- .NET SDK 8;
- Node.js zgodny z Angular 21 (Dockerfile używa Node 20);
- npm;
- SQL Server albo Docker Desktop.

## Uruchomienie

```powershell
docker compose up -d gieudexpol-db
dotnet run --project GieudexPol.API
cd GieudexPol.Frontend
npm install
npm start
```

Adres API zależy od `launchSettings.json`; frontend w development zwykle działa przez Angular dev server. Dla pełnego Compose nginx jest pod `http://localhost`, a API pod `http://localhost:5010`.

Przed ponownym uruchomieniem API sprawdź, czy port procesu nie jest już zajęty.
