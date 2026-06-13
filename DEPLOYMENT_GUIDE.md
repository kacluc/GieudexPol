# GieudexPol: wdrożenie i uruchamianie

Aktualna dokumentacja wdrożeniowa została uporządkowana w:

- [rozwój lokalny](docs/deployment/local-development.md);
- [Docker](docs/deployment/docker.md);
- [baza danych](docs/deployment/database.md);
- [migracje](docs/deployment/migrations.md);
- [diagram wdrożenia](docs/uml/deployment-diagram.puml).

Projekt używa .NET 8, Angular 21 i SQL Server 2022. Bieżący `docker-compose.yml` wystawia nginx na portach `80/443`, API na `5010/5011`, a SQL Server na `1433`.

Bezpieczne zatrzymanie:

```powershell
docker compose down
```

Polecenie nie usuwa nazwanego wolumenu `gieudexpol-data`. Nie używaj opcji `-v`, jeżeli baza ma zostać zachowana.
