using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

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
                return Ok(MapToDto(transaction));
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
        public async Task<IActionResult> GetUserTransactions(int userId)
        {
            var transactions = await _transactionService.GetUserTransactions(userId);
            return Ok(transactions.Select(MapToDto));
        }

        private static TransactionResponseDto MapToDto(GieudexPol.Domain.Entities.Transaction transaction)
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
}
