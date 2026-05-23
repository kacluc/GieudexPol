using GieudexPol.Application.DTOs;
using GieudexPol.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    public class FavoriteCurrencyController : ControllerBase
    {
        private readonly FavoriteCurrencyService _service;

        public FavoriteCurrencyController(
            FavoriteCurrencyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var favorites = await _service
                .GetFavoritesAsync();

            return Ok(favorites);
        }

        [HttpPost]
        public async Task<IActionResult> Add(
            AddFavoriteCurrencyDto dto)
        {
            await _service.AddFavoriteAsync(dto);

            return Ok();
        }

        [HttpDelete("{currencyCode}")]
        public async Task<IActionResult> Remove(
            string currencyCode)
        {
            await _service.RemoveFavoriteAsync(currencyCode);

            return Ok();
        }
    }
}