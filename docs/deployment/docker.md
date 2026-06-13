# Docker

`docker-compose.yml` definiuje:

- `gieudexpol-db`: SQL Server 2022 Express, port `1433`;
- `gieudexpol-api`: obraz budowany z głównego `Dockerfile`, porty `5010:80` i `5011:443`;
- `gieudexpol-nginx`: statyczny frontend i proxy, porty `80/443`;
- nazwany wolumen `gieudexpol-data`;
- sieć `gieudexpol-network`.

```powershell
docker compose up --build
docker compose ps
docker compose logs -f gieudexpol-api
docker compose down
```

`docker compose down` zachowuje dane. `docker compose down -v` usuwa wolumen i nie jest właściwym sposobem zwykłego zatrzymania środowiska.

Hasła i klucz JWT zapisane w Compose są wartościami developerskimi i przed wdrożeniem muszą zostać zastąpione sekretami.
