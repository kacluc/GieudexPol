# Dokumentacja GieudexPol

Dokumentacja opisuje stan kodu z 13 czerwca 2026 r. Foldery `UML/` i `Specyfikacje/` zawierają starsze materiały; bieżącym indeksem jest ten plik.

## Przekrój systemu

- [opis projektu](overview/00-project-overview.md)
- [architektura](overview/01-architecture.md)
- [stos technologiczny](overview/02-technology-stack.md)
- [model domenowy](overview/03-domain-model.md)

## Moduły

| Moduł | Specyfikacja | Reguły | API | Dane |
|---|---|---|---|---|
| Kursy walut | [opis](modules/exchange-rates/specification.md) | [reguły](modules/exchange-rates/business-rules.md) | [API](modules/exchange-rates/api.md) | [model](modules/exchange-rates/data-model.md) |
| Portfele | [opis](modules/wallets/specification.md) | [reguły](modules/wallets/business-rules.md) | [API](modules/wallets/api.md) | [model](modules/wallets/data-model.md) |
| Szybka wymiana | [opis](modules/instant-exchange/specification.md) | [reguły](modules/instant-exchange/business-rules.md) | [API](modules/instant-exchange/api.md) | [model](modules/instant-exchange/data-model.md) |
| Rynek walut | [opis](modules/order-book/specification.md) | [reguły](modules/order-book/business-rules.md) | [API](modules/order-book/api.md) | [model](modules/order-book/data-model.md) |
| Prowizje i skarbiec | [opis](modules/fees-and-treasury/specification.md) | [reguły](modules/fees-and-treasury/business-rules.md) | [API](modules/fees-and-treasury/api.md) | [model](modules/fees-and-treasury/data-model.md) |
| Konta systemowe | [opis](modules/system-accounts/specification.md) | [reguły](modules/system-accounts/business-rules.md) | [API](modules/system-accounts/api.md) | [model](modules/system-accounts/data-model.md) |
| Alerty | [opis](modules/alerts/specification.md) | [reguły](modules/alerts/business-rules.md) | [API](modules/alerts/api.md) | [model](modules/alerts/data-model.md) |
| Użytkownicy i auth | [opis](modules/users-and-auth/specification.md) | [reguły](modules/users-and-auth/business-rules.md) | [API](modules/users-and-auth/api.md) | [model](modules/users-and-auth/data-model.md) |
| Administracja | [opis](modules/admin/specification.md) | [reguły](modules/admin/business-rules.md) | [API](modules/admin/api.md) |

## Deployment i testy

- [rozwój lokalny](deployment/local-development.md)
- [Docker](deployment/docker.md)
- [baza danych](deployment/database.md)
- [migracje](deployment/migrations.md)
- [strategia testów](testing/test-strategy.md)
- [testy backendu](testing/backend-tests.md)
- [testy frontendu](testing/frontend-tests.md)

## Diagramy globalne

- [kontekst systemu](uml/system-context.puml)
- [komponenty](uml/component-diagram.puml)
- [wdrożenie](uml/deployment-diagram.puml)
- [przypadki użycia](uml/main-use-case-diagram.puml)

## PlantUML w VS Code

1. Zainstaluj rozszerzenie PlantUML.
2. Otwórz plik `.puml`.
3. Uruchom polecenie `PlantUML: Preview Current Diagram`.
4. Jeżeli wybrany tryb rozszerzenia wymaga Java lub Graphviz, skonfiguruj je zgodnie z dokumentacją rozszerzenia.

Diagramy używają wyłącznie standardowej składni PlantUML, bez Mermaid, `!includeurl` i bibliotek C4.
