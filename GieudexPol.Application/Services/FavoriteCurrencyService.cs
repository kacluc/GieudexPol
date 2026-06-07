using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Services
{
    public class FavoriteCurrencyService
    {
        private readonly IFavoriteCurrencyRepository _repository;

        public FavoriteCurrencyService(
            IFavoriteCurrencyRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FavoriteCurrencyDto>> GetFavoritesAsync()
        {
            var favorites = await _repository.GetFavoritesAsync();

            return favorites.Select(x => new FavoriteCurrencyDto
            {
                CurrencyCode = x.CurrencyCode
            }).ToList();
        }

        public async Task AddFavoriteAsync(
            AddFavoriteCurrencyDto dto)
        {
            var currencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();

            if (!string.Equals(currencyCode, TradingCurrencyCatalog.BaseCurrencySymbol, StringComparison.OrdinalIgnoreCase) &&
                !TradingCurrencyCatalog.Contains(currencyCode))
            {
                throw new InvalidOperationException("Wybrana waluta nie jest dostepna na wykresach kursow.");
            }

            var exists = await _repository
                .ExistsAsync(currencyCode);

            if (exists)
                return;

            var favorite = new FavoriteCurrency
            {
                CurrencyCode = currencyCode
            };

            await _repository.AddAsync(favorite);
        }

        public async Task RemoveFavoriteAsync(
            string currencyCode)
        {
            await _repository.RemoveAsync(currencyCode.Trim().ToUpperInvariant());
        }
    }
}
