using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/admin/rate-source-orders")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminRateSourceOrdersController : ControllerBase
    {
        private readonly IOrderBookService _orderBookService;

        public AdminRateSourceOrdersController(IOrderBookService orderBookService)
        {
            _orderBookService = orderBookService;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create(
            [FromBody] CreateRateSourceOrderRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderBookService.PlaceRateSourceOrderAsync(
                    request.RateSourceCode,
                    request,
                    cancellationToken);
                return Created("/api/orders/my", order);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new { message = exception.Message });
            }
        }
    }
}
