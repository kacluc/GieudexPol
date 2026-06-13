# Stos technologiczny

| Obszar | Stan w repozytorium |
|---|---|
| Runtime backendu | .NET 8 (`net8.0`) |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Baza | SQL Server 2022 |
| Auth | JWT Bearer, HMAC SHA-256 |
| Frontend | Angular 21.2 |
| Język frontendu | TypeScript 5.9 |
| Reaktywność | RxJS 7.8 |
| Testy backendu | xUnit, Moq, EF InMemory |
| Testy frontendu | Angular CLI + Vitest |
| Kontenery | Docker, Docker Compose, nginx |
| Dokumentacja diagramów | PlantUML |

Dockerfile buduje frontend na Node 20 i backend na obrazach .NET 8. `docker-compose.yml` używa SQL Server 2022 Express.
