using GieudexPol.Application.Interfaces;
using GieudexPol.Application.DTOs;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IUserRepository _userRepository;

        public WalletsController(
            IWalletService walletService,
            IUserRepository userRepository)
        {
            _walletService = walletService;
            _userRepository = userRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserWallets(int userId)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Id != userId)
            {
                return Forbid();
            }

            var wallets = await _walletService.GetUserWalletsAsync(userId);
            return Ok(wallets.Select(WalletResponse.FromWallet));
        }

        [HttpGet("available-currencies")]
        public async Task<IActionResult> GetAvailableCurrencies(
            [FromQuery] int userId,
            CancellationToken cancellationToken)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Id != userId)
            {
                return Forbid();
            }

            var currencies = await _walletService.GetAvailableWalletCurrenciesAsync(userId, cancellationToken);
            return Ok(currencies.Select(WalletCurrencyResponse.FromCurrency));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById(int id)
        {
            var wallet = await _walletService.GetByIdAsync(id);
            if (wallet == null)
            {
                return NotFound();
            }

            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            return currentUser.Id == wallet.UserId
                ? Ok(WalletResponse.FromWallet(wallet))
                : Forbid();
        }

        [HttpPost("user/{userId}/currencies/{currencyId}")]
        public async Task<IActionResult> AddCurrencyWallet(
            int userId,
            int currencyId,
            CancellationToken cancellationToken)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Id != userId)
            {
                return Forbid();
            }

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
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            return await AddCurrencyWallet(currentUser.Id, wallet.CurrencyId, cancellationToken);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWallet(int id, [FromBody] Wallet wallet)
        {
            if (id != wallet.Id)
            {
                return BadRequest();
            }

            var currentUser = await GetAuthenticatedUserAsync();
            var persistedWallet = await _walletService.GetByIdAsync(id);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (persistedWallet == null)
            {
                return NotFound();
            }

            if (persistedWallet.UserId != currentUser.Id)
            {
                return Forbid();
            }

            wallet.UserId = currentUser.Id;
            await _walletService.UpdateAsync(wallet);
            return NoContent();
        }

        [HttpPost("trade")]
        public async Task<IActionResult> ExecuteTrade([FromQuery] int userId, [FromBody] TradeRequest request)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Id != userId)
            {
                return Forbid();
            }

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
                    result.RateSource,
                    result.AppliedRate,
                    result.FeeAmount,
                    result.FeeCurrency,
                    result.ExchangeExecutionId,
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
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Id != userId)
            {
                return Forbid();
            }

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
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Id != userId)
            {
                return Forbid();
            }

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

        [HttpPost("/api/wallet/exchange/preview")]
        public async Task<IActionResult> PreviewExchange(
            [FromBody] ExchangePreviewRequestDto request,
            CancellationToken cancellationToken)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            try
            {
                return Ok(await _walletService.PreviewTradeAsync(
                    currentUser.Id,
                    request.FromCurrencyId,
                    request.Amount,
                    request.ToCurrencyId,
                    cancellationToken));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        private async Task<User?> GetAuthenticatedUserAsync()
        {
            var authIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(authIdValue, out var authId)
                ? await _userRepository.GetByAuthIdAsync(authId)
                : null;
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
