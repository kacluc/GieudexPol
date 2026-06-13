# Konta systemowe

## Cel

Konta systemowe reprezentują płynność źródeł kursów oraz skarbiec platformy. Mają zwykłe portfele, lecz nie są zwykłymi użytkownikami.

## Typy kont

- `RegularUser`
- `AdminUser`
- `SuperAdminUser`
- `RateSourceSystem`
- `PlatformTreasury`

Każde aktywne `RateSource` otrzymuje konto `system_{code}` i powiązanie `SystemUserId`. Skarbiec ma nazwę `system_platform_treasury`.

## Możliwości

Konto źródła dostarcza płynność szybkiej wymianie i może składać zlecenia przez endpoint admina. PlatformTreasury zbiera prowizje. Oba typy są widoczne w dedykowanym panelu admina.

## Ograniczenia

Konta systemowe nie mogą logować się, być odbiorcą zwykłego transferu ani pojawiać się w rankingu. Publiczny order book nie ujawnia właścicieli.
