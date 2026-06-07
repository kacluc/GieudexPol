using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillSyntheticExchangeRateSpread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE exchangeRate
                SET
                    MidPrice = ROUND(referenceRate.Value, 4),
                    BuyPrice = CASE
                        WHEN roundedRate.BuyPrice < roundedRate.SellPrice
                            THEN roundedRate.BuyPrice
                        WHEN ROUND(referenceRate.Value, 4) > 0.0001
                            THEN ROUND(referenceRate.Value, 4) - 0.0001
                        ELSE 0.0001
                    END,
                    SellPrice = CASE
                        WHEN roundedRate.BuyPrice < roundedRate.SellPrice
                            THEN roundedRate.SellPrice
                        ELSE ROUND(referenceRate.Value, 4) + 0.0001
                    END
                FROM ExchangeRates exchangeRate
                INNER JOIN RateSources rateSource
                    ON rateSource.Id = exchangeRate.RateSourceId
                CROSS APPLY (
                    SELECT COALESCE(
                        exchangeRate.MidPrice,
                        (exchangeRate.BuyPrice + exchangeRate.SellPrice) / 2
                    ) AS Value
                ) referenceRate
                CROSS APPLY (
                    SELECT
                        ROUND(referenceRate.Value * 0.99, 4) AS BuyPrice,
                        ROUND(referenceRate.Value * 1.01, 4) AS SellPrice
                ) roundedRate
                WHERE rateSource.Code IN ('ECB', 'BOE', 'RIKSBANK', 'CNB', 'NORGES', 'BNR')
                  AND exchangeRate.BuyPrice = exchangeRate.SellPrice;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Existing reference rates cannot be restored reliably after applying a spread.
        }
    }
}
