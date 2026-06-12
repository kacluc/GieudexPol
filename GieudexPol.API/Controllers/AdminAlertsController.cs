using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/admin/alerts")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminAlertsController : ControllerBase
    {
        private readonly IAlertEvaluationService _alertEvaluationService;

        public AdminAlertsController(IAlertEvaluationService alertEvaluationService)
        {
            _alertEvaluationService = alertEvaluationService;
        }

        [HttpPost("evaluate")]
        public async Task<ActionResult<AlertEvaluationResult>> Evaluate(
            [FromBody] AlertEvaluationRequest? request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request?.AlertId is int alertId)
                {
                    return Ok(await _alertEvaluationService.EvaluateAlertAsync(
                        alertId,
                        cancellationToken));
                }

                if (!string.IsNullOrWhiteSpace(request?.CurrencyCode) ||
                    !string.IsNullOrWhiteSpace(request?.RateSourceCode))
                {
                    return Ok(await _alertEvaluationService.EvaluateAlertsForRateAsync(
                        request?.CurrencyCode,
                        request?.RateSourceCode,
                        cancellationToken));
                }

                return Ok(await _alertEvaluationService.EvaluateAllActiveAlertsAsync(
                    cancellationToken));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }
    }
}
