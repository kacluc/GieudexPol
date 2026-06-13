# Reguły kont systemowych

- `Role = System`, natomiast `AccountType` rozróżnia źródło i skarbiec.
- Hasło ma techniczną wartość `!SYSTEM_ACCOUNT_NO_LOGIN!`.
- `IdentityService` odrzuca logowanie obu typów systemowych.
- Seeder tworzy konta deterministycznie i idempotentnie.
- Każde konto systemowe ma portfel dla każdej aktywnej waluty.
- Domyślna płynność źródła: PLN 1 000 000; EUR/USD 400 000; GBP/CHF 200 000; JPY/KRW 50 000 000; pozostałe 500 000.
- Skarbiec zaczyna z saldem 0.
- Ponowne uruchomienie seedera nie uzupełnia istniejącego portfela do wartości początkowej.
- Transfer do konta systemowego jest blokowany po stronie backendu.
- Ranking odfiltrowuje `RateSourceSystem` i `PlatformTreasury`.
- Adminowe zlecenie wymaga aktywnego źródła i poprawnego powiązania systemowego.
