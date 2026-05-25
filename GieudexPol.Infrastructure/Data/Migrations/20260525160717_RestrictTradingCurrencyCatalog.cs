using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestrictTradingCurrencyCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM [ExchangeRates]
                WHERE [CurrencyId] IN
                (
                    SELECT [Id]
                    FROM [Currencies]
                    WHERE [Symbol] NOT IN
                    (
                        'PLN', 'EUR', 'USD', 'CHF', 'GBP', 'HUF', 'CZK', 'DKK', 'SEK',
                        'NOK', 'RON', 'TRY', 'UAH', 'AUD', 'CAD', 'JPY', 'KRW'
                    )
                );

                DELETE FROM [Currencies]
                WHERE [Symbol] NOT IN
                (
                    'PLN', 'EUR', 'USD', 'CHF', 'GBP', 'HUF', 'CZK', 'DKK', 'SEK',
                    'NOK', 'RON', 'TRY', 'UAH', 'AUD', 'CAD', 'JPY', 'KRW'
                )
                AND NOT EXISTS (SELECT 1 FROM [Wallets] WHERE [Wallets].[CurrencyId] = [Currencies].[Id])
                AND NOT EXISTS (SELECT 1 FROM [Transactions] WHERE [Transactions].[CurrencyId] = [Currencies].[Id])
                AND NOT EXISTS (SELECT 1 FROM [UserAlerts] WHERE [UserAlerts].[CurrencyId] = [Currencies].[Id]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Removed imported rates cannot be reconstructed by a rollback.
        }
    }
}
