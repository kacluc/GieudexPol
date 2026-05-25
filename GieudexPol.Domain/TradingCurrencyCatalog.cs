namespace GieudexPol.Domain
{
    public static class TradingCurrencyCatalog
    {
        public const string BaseCurrencySymbol = "PLN";

        public static readonly string[] Symbols =
        [
            "EUR",
            "USD",
            "CHF",
            "GBP",
            "HUF",
            "CZK",
            "DKK",
            "SEK",
            "NOK",
            "RON",
            "TRY",
            "UAH",
            "AUD",
            "CAD",
            "JPY",
            "KRW"
        ];

        public static bool Contains(string symbol)
        {
            return Symbols.Contains(symbol, StringComparer.OrdinalIgnoreCase);
        }
    }
}
