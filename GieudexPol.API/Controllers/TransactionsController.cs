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
                var senderEmail = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrWhiteSpace(senderEmail))
                {
                    return Unauthorized();
                }

                var sender = await _userRepository.GetByUsernameAsync(senderEmail);
                if (sender == null)
                {
                    return Unauthorized();
                }

                request.SenderId = sender.Id;
                var transaction = await _transactionService.CreateTransfer(request);
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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null || int.Parse(currentUserId) != userId)
            {
                return Unauthorized();
            }

            var paginatedResult = await _transactionService.GetUserTransactions(
                userId, pageNumber, pageSize, transactionType, currencyId, startDate, endDate);
            
            return Ok(paginatedResult);
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
