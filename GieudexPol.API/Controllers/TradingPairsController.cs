using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/trading-pairs")]
    [Authorize]
    public class TradingPairsController : ControllerBase
    {
        private readonly IOrderBookService _orderBookService;

        public TradingPairsController(IOrderBookService orderBookService)
        {
            _orderBookService = orderBookService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await _orderBookService.GetTradingPairsAsync(cancellationToken));
        }
    }
}
