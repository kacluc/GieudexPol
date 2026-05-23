using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
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
            var exists = await _repository
                .ExistsAsync(dto.CurrencyCode);

            if (exists)
                return;

            var favorite = new FavoriteCurrency
            {
                CurrencyCode = dto.CurrencyCode
            };

            await _repository.AddAsync(favorite);
        }

        public async Task RemoveFavoriteAsync(
            string currencyCode)
        {
            await _repository.RemoveAsync(currencyCode);
        }
    }
}