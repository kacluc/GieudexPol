using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            return Ok(wallets.Select(WalletResponse.FromWallet));
        }

        [HttpGet("available-currencies")]
        public async Task<IActionResult> GetAvailableCurrencies(
            [FromQuery] int userId,
            CancellationToken cancellationToken)
        {
            var currencies = await _walletService.GetAvailableWalletCurrenciesAsync(userId, cancellationToken);
            return Ok(currencies.Select(WalletCurrencyResponse.FromCurrency));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById(int id)
        {
            var wallet = await _walletService.GetByIdAsync(id);
            return wallet == null ? NotFound() : Ok(WalletResponse.FromWallet(wallet));
        }

        [HttpPost("user/{userId}/currencies/{currencyId}")]
        public async Task<IActionResult> AddCurrencyWallet(
            int userId,
            int currencyId,
            CancellationToken cancellationToken)
        {
            try
            {
                var wallet = await _walletService.AddCurrencyWalletAsync(userId, currencyId, cancellationToken);
                return CreatedAtAction(nameof(GetWalletById), new { id = wallet.Id }, WalletResponse.FromWallet(wallet));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "Wallet creation failed", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] Wallet wallet, CancellationToken cancellationToken)
        {
            return await AddCurrencyWallet(wallet.UserId, wallet.CurrencyId, cancellationToken);
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

        [HttpPost("trade")]
        public async Task<IActionResult> ExecuteTrade([FromQuery] int userId, [FromBody] TradeRequest request)
        {
            try
            {
                var result = await _walletService.ExecuteTradeTransactionAsync(
                    userId,
                    request.FromCurrencyId,
                    request.AmountFrom,
                    request.ToCurrencyId,
                    HttpContext.RequestAborted);

                return Ok(new
                {
                    success = true,
                    message = "Trade executed successfully.",
                    result.AmountTo,
                    result.FromCurrency,
                    result.ToCurrency,
                    result.FromRateToPln,
                    result.ToRateToPln,
                    result.SellRateSource,
                    result.BuyRateSource,
                    result.EffectiveDate
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Invalid amount", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "Transaction failed", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromQuery] int userId, [FromBody] DepositRequest request)
        {
            try
            {
                await _walletService.DepositAsync(userId, request.CurrencyId, request.Amount);
                return Ok(new { success = true, message = "Deposit executed successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Invalid amount", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "Deposit failed", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromQuery] int userId, [FromBody] WithdrawRequest request)
        {
            try
            {
                await _walletService.WithdrawAsync(userId, request.CurrencyId, request.Amount);
                return Ok(new { success = true, message = "Withdrawal executed successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Invalid amount", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "Withdrawal failed", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }
    }

    public class TradeRequest
    {
        public int FromCurrencyId { get; set; }
        public decimal AmountFrom { get; set; }
        public int ToCurrencyId { get; set; }
    }

    public class WalletResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public decimal Balance { get; set; }
        public decimal ReservedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public WalletCurrencyResponse? Currency { get; set; }

        public static WalletResponse FromWallet(Wallet wallet)
        {
            return new WalletResponse
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                CurrencyId = wallet.CurrencyId,
                Balance = wallet.Balance,
                ReservedBalance = wallet.ReservedBalance,
                AvailableBalance = wallet.AvailableBalance,
                Currency = wallet.Currency == null ? null : WalletCurrencyResponse.FromCurrency(wallet.Currency)
            };
        }
    }

    public class WalletCurrencyResponse
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public static WalletCurrencyResponse FromCurrency(Currency currency)
        {
            return new WalletCurrencyResponse
            {
                Id = currency.Id,
                Symbol = currency.Symbol,
                Name = currency.Name,
                IsActive = currency.IsActive
            };
        }
    }

    public class DepositRequest
    {
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
    }

    public class WithdrawRequest
    {
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
    }
}
