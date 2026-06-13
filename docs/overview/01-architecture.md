# Architektura

Rozwiązanie jest podzielone na cztery projekty backendowe i osobny frontend.

- `GieudexPol.Domain`: encje, enumy i metody domenowe portfela.
- `GieudexPol.Application`: DTO, interfejsy i serwisy aplikacyjne, m.in. prowizje oraz transfery.
- `GieudexPol.Infrastructure`: EF Core, repozytoria, integracje kursowe, matching, alerty i seeding.
- `GieudexPol.API`: kontrolery, JWT, DI oraz workery.
- `GieudexPol.Frontend`: Angular SPA.

Zależności biegną od API i Infrastructure do Application/Domain. `ApplicationDbContext` jest centralnym modelem zapisu. Operacje wymagające spójności, jak matching i szybka wymiana, używają transakcji o izolacji `Serializable` dla relacyjnej bazy.

Workery:

- `ExchangeRateStartupSyncService`: migracje, seed i synchronizacja startowa;
- `AlertMonitoringWorker`: cykliczna ewaluacja alertów kursowych i rynkowych.

Zobacz [diagram komponentów](../uml/component-diagram.puml).
