using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrenciesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var currencies = await _currencyService.GetAllAsync();
            return Ok(currencies);
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetCurrencyBySymbol(string symbol)
        {
            var currency = await _currencyService.GetBySymbolAsync(symbol);
            if (currency == null)
            {
                return NotFound();
            }
            return Ok(currency);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCurrency([FromBody] Currency currency)
        {
            await _currencyService.AddAsync(currency);
            return CreatedAtAction(nameof(GetCurrencyBySymbol), new { symbol = currency.Symbol }, currency);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurrency(int id, [FromBody] Currency currency)
        {
            if (id != currency.Id)
            {
                return BadRequest();
            }
            await _currencyService.UpdateAsync(currency);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            var currency = await _currencyService.GetByIdAsync(id);
            if (currency == null)
            {
                return NotFound();
            }
            await _currencyService.DeleteAsync(currency);
            return NoContent();
        }
    }
}
