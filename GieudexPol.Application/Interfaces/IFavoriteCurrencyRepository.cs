using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IFavoriteCurrencyRepository
    {
        Task<List<FavoriteCurrency>> GetFavoritesAsync();

        Task AddAsync(FavoriteCurrency favorite);

        Task RemoveAsync(string currencyCode);

        Task<bool> ExistsAsync(string currencyCode);
    }
}