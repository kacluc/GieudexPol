using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GieudexPol.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IUserRepository _userRepository;

        public TransactionsController(
            ITransactionService transactionService,
            IUserRepository userRepository)
        {
            _transactionService = transactionService;
            _userRepository = userRepository;
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> CreateTransfer([FromBody] TransferRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sender = await GetAuthenticatedUserAsync();
                if (sender == null)
                {
                    return Unauthorized();
                }

                var transaction = await _transactionService.CreateTransfer(sender.Id, request);
                return Ok(MapToResponseDto(transaction));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTransactions(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? transactionType = null,
            [FromQuery] int? currencyId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
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

            var paginatedResult = await _transactionService.GetUserTransactions(
                userId, pageNumber, pageSize, transactionType, currencyId, startDate, endDate);
            
            return Ok(paginatedResult);
        }

        private async Task<Domain.Entities.User?> GetAuthenticatedUserAsync()
        {
            var authIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(authIdValue, out var authId))
            {
                return null;
            }

            return await _userRepository.GetByAuthIdAsync(authId);
        }

        private static TransactionResponseDto MapToResponseDto(Domain.Entities.Transaction transaction)
        {
            return new TransactionResponseDto
            {
                Id = transaction.Id,
                SenderId = transaction.SenderId,
                ReceiverId = transaction.ReceiverId,
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                Status = transaction.Status,
                TransactionType = transaction.TransactionType,
                AppliedFee = transaction.AppliedFee,
                TransactionFeeId = transaction.TransactionFeeId,
                Timestamp = transaction.Timestamp
            };
        }
    }

    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal AppliedFee { get; set; }
        public Guid? TransactionFeeId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
