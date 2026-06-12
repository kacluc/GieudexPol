using System.Security.Claims;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderBookService _orderBookService;
        private readonly IUserRepository _userRepository;

        public OrdersController(
            IOrderBookService orderBookService,
            IUserRepository userRepository)
        {
            _orderBookService = orderBookService;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateOrderRequestDto request,
            CancellationToken cancellationToken)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var order = await _orderBookService.PlaceOrderAsync(
                    user.Id,
                    request,
                    cancellationToken);
                return Created("/api/orders/my", order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(await _orderBookService.GetMyOrdersAsync(
                user.Id,
                cancellationToken));
        }

        [HttpDelete("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(
            int id,
            CancellationToken cancellationToken)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                await _orderBookService.CancelOrderAsync(
                    user.Id,
                    id,
                    cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        private async Task<Domain.Entities.User?> GetAuthenticatedUserAsync()
        {
            var authIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(authIdValue, out var authId)
                ? await _userRepository.GetByAuthIdAsync(authId)
                : null;
        }
    }
}
