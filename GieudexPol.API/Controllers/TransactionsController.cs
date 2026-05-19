using GieudexPol.Application.DTOs;
using GieudexPol.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
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
                var transaction = await _transactionService.CreateTransfer(request);
                return Ok(transaction);
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
            return Ok(transactions);
        }
    }
}
