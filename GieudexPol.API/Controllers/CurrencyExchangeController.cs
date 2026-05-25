using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers;

[ApiController]
[Route("api/exchange")]
[Authorize]
public class CurrencyExchangeController : ControllerBase
{
    private readonly ICurrencyExchangeSimulationService _simulationService;

    public CurrencyExchangeController(
        ICurrencyExchangeSimulationService simulationService)
    {
        _simulationService = simulationService;
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateExchange(
        [FromBody] CurrencyExchangeSimulationRequestDto request)
    {
        try
        {
            var result =
                await _simulationService
                    .SimulateExchangeAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
