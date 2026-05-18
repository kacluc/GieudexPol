using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserWallets(int userId)
        {
            var wallets = await _walletService.GetUserWalletsAsync(userId);
            if (wallets == null)
            {
                return NotFound();
            }
            return Ok(wallets.Select(WalletResponse.FromWallet));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById(int id)
        {
            var wallet = await _walletService.GetByIdAsync(id);
            if (wallet == null)
            {
                return NotFound();
            }
            return Ok(WalletResponse.FromWallet(wallet));
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] Wallet wallet)
        {
            await _walletService.AddAsync(wallet);
            return CreatedAtAction(nameof(GetWalletById), new { id = wallet.Id }, wallet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWallet(int id, [FromBody] Wallet wallet)
        {
            if (id != wallet.Id)
            {
                return BadRequest();
            }
            await _walletService.UpdateAsync(wallet);
            return NoContent();
        }

        /// <summary>
        /// Executes a trade transaction by debiting the source wallet and crediting the destination wallet.
        /// </summary>
        [HttpPost("trade")]
        public async Task<IActionResult> ExecuteTrade([FromQuery] int userId, [FromBody] TradeRequest request)
        {
            try
            {
                await _walletService.ExecuteTradeTransactionAsync(
                    userId, 
                    request.FromCurrencyId, 
                    request.AmountFrom, 
                    request.ToCurrencyId, 
                    request.AmountTo
                );
                return Ok("Trade executed successfully.");
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("Insufficient funds", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Niewystarczające środki", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Transaction failed", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }
    }

    // Ta klasa musi być tutaj, aby kontroler widział strukturę przesyłanego obiektu JSON
    public class TradeRequest
    {
        public int FromCurrencyId { get; set; }
        public decimal AmountFrom { get; set; }
        public int ToCurrencyId { get; set; }
        public decimal AmountTo { get; set; }
    }

    public class WalletResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public decimal Balance { get; set; }
        public WalletCurrencyResponse? Currency { get; set; }

        public static WalletResponse FromWallet(Wallet wallet)
        {
            return new WalletResponse
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                CurrencyId = wallet.CurrencyId,
                Balance = wallet.Balance,
                Currency = wallet.Currency == null
                    ? null
                    : new WalletCurrencyResponse
                    {
                        Id = wallet.Currency.Id,
                        Symbol = wallet.Currency.Symbol,
                        Name = wallet.Currency.Name,
                        IsActive = wallet.Currency.IsActive
                    }
            };
        }
    }

    public class WalletCurrencyResponse
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
