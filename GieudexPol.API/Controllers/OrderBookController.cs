using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/order-book")]
    [Authorize]
    public class OrderBookController : ControllerBase
    {
        private readonly IOrderBookService _orderBookService;

        public OrderBookController(IOrderBookService orderBookService)
        {
            _orderBookService = orderBookService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string baseCurrencyCode,
            [FromQuery] string quoteCurrencyCode,
            [FromQuery] int depth = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _orderBookService.GetOrderBookAsync(
                    baseCurrencyCode,
                    quoteCurrencyCode,
                    depth,
                    cancellationToken));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
