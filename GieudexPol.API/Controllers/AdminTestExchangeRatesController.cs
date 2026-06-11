using GieudexPol.Application.DTOs;
using GieudexPol.Application.Exceptions;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/admin/test-exchange-rates")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminTestExchangeRatesController : ControllerBase
    {
        private readonly IAdminTestExchangeRateService _service;

        public AdminTestExchangeRatesController(IAdminTestExchangeRateService service)
        {
            _service = service;
        }

        [HttpGet("sources")]
        public async Task<ActionResult<IReadOnlyList<AdminTestRateSourceDto>>> GetSources(
            CancellationToken cancellationToken)
        {
            return Ok(await _service.GetSourcesAsync(cancellationToken));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminTestExchangeRateDto>>> GetRates(
            [FromQuery] string? rateSourceCode,
            [FromQuery] int? currencyId,
            [FromQuery] string? currencyCode,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _service.GetRatesAsync(
                    rateSourceCode,
                    currencyId,
                    currencyCode,
                    dateFrom,
                    dateTo,
                    cancellationToken));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (DevelopmentRateSourceNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (ProtectedExchangeRateException exception)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminTestExchangeRateDto>> GetRate(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var rate = await _service.GetRateAsync(id, cancellationToken);
                return rate == null
                    ? NotFound(new { message = "Testowy kurs nie istnieje." })
                    : Ok(rate);
            }
            catch (DevelopmentRateSourceNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (ProtectedExchangeRateException exception)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<AdminTestExchangeRateDto>> CreateRate(
            [FromBody] CreateTestExchangeRateDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var rate = await _service.CreateRateAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetRate), new { id = rate.Id }, rate);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (DevelopmentRateSourceNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (ProtectedExchangeRateException exception)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
            }
            catch (TestExchangeRateConflictException exception)
            {
                return Conflict(new { message = exception.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AdminTestExchangeRateDto>> UpdateRate(
            int id,
            [FromBody] UpdateTestExchangeRateDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var rate = await _service.UpdateRateAsync(id, request, cancellationToken);
                return rate == null
                    ? NotFound(new { message = "Kurs nie istnieje." })
                    : Ok(rate);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (DevelopmentRateSourceNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (ProtectedExchangeRateException exception)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
            }
            catch (TestExchangeRateConflictException exception)
            {
                return Conflict(new { message = exception.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRate(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _service.DeleteRateAsync(id, cancellationToken);
                return deleted
                    ? NoContent()
                    : NotFound(new { message = "Kurs nie istnieje." });
            }
            catch (DevelopmentRateSourceNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (ProtectedExchangeRateException exception)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
            }
        }
    }
}
