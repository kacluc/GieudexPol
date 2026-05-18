using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace GieudexPol.Infrastructure.ExternalServices.Nbp
{
    public class NbpExchangeRateClient : IExternalExchangeRateClient
    {
        private readonly HttpClient _httpClient;

        public string SourceCode => "NBP";
        public string SourceName => "Narodowy Bank Polski";
        public int MaxRangeDays => 93;

        public NbpExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"exchangerates/tables/C/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}/?format=json";
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<ExternalExchangeRateTableDto>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"NBP rejected range {from:yyyy-MM-dd} - {to:yyyy-MM-dd}: {message}");
            }

            response.EnsureSuccessStatusCode();

            var tables = await response.Content.ReadFromJsonAsync<List<ExternalExchangeRateTableDto>>(
                cancellationToken: cancellationToken);

            return tables ?? new List<ExternalExchangeRateTableDto>();
        }
    }
}
