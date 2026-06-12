using System.Security.Claims;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/trading-alerts")]
    [Authorize]
    public class TradingAlertsController : ControllerBase
    {
        private readonly IUserTradingAlertService _alertService;
        private readonly ITradingAlertEvaluationService _evaluationService;
        private readonly IUserRepository _userRepository;

        public TradingAlertsController(
            IUserTradingAlertService alertService,
            ITradingAlertEvaluationService evaluationService,
            IUserRepository userRepository)
        {
            _alertService = alertService;
            _evaluationService = evaluationService;
            _userRepository = userRepository;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IReadOnlyList<UserTradingAlertDto>>> GetMyAlerts(
            CancellationToken cancellationToken)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var alerts = await _alertService.GetUserAlertsAsync(
                user.Id,
                cancellationToken);
            return Ok(alerts.Select(MapToDto));
        }

        [HttpPost]
        public async Task<ActionResult<UserTradingAlertDto>> Create(
            [FromBody] UserTradingAlertCreateDto request,
            CancellationToken cancellationToken)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var alert = new UserTradingAlert
            {
                UserId = user.Id,
                TradingPairId = request.TradingPairId,
                EventType = request.EventType,
                Direction = request.Direction,
                TargetPrice = request.TargetPrice,
                MinimumAmount = request.MinimumAmount
            };

            try
            {
                await _alertService.CreateAsync(alert, cancellationToken);
                await _evaluationService.EvaluateAlertAsync(alert.Id, cancellationToken);
                alert = await _alertService.GetByIdAsync(alert.Id, cancellationToken) ?? alert;
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }

            return CreatedAtAction(nameof(GetMyAlerts), MapToDto(alert));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UserTradingAlertUpdateDto request,
            CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var alert = await _alertService.GetByIdAsync(id, cancellationToken);
            if (alert == null)
            {
                return NotFound();
            }

            if (alert.UserId != user.Id)
            {
                return Forbid();
            }

            alert.TradingPairId = request.TradingPairId;
            alert.EventType = request.EventType;
            alert.Direction = request.Direction;
            alert.TargetPrice = request.TargetPrice;
            alert.MinimumAmount = request.MinimumAmount;
            alert.IsActive = request.IsActive;

            try
            {
                await _alertService.UpdateAsync(alert, cancellationToken);
                if (alert.IsActive)
                {
                    await _evaluationService.EvaluateAlertAsync(alert.Id, cancellationToken);
                }
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var alert = await _alertService.GetByIdAsync(id, cancellationToken);
            if (alert == null)
            {
                return NotFound();
            }

            if (alert.UserId != user.Id)
            {
                return Forbid();
            }

            await _alertService.DeleteAsync(alert, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id:int}/acknowledge")]
        public async Task<IActionResult> Acknowledge(
            int id,
            CancellationToken cancellationToken)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var acknowledged = await _alertService.AcknowledgeAsync(
                id,
                user.Id,
                cancellationToken);
            return acknowledged ? NoContent() : NotFound();
        }

        private async Task<User?> GetAuthenticatedUserAsync()
        {
            var authIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(authIdValue, out var authId)
                ? await _userRepository.GetByAuthIdAsync(authId)
                : null;
        }

        private static UserTradingAlertDto MapToDto(UserTradingAlert alert)
        {
            return new UserTradingAlertDto
            {
                Id = alert.Id,
                TradingPairId = alert.TradingPairId,
                Pair = alert.TradingPair.BaseCurrency.Symbol + "/" +
                       alert.TradingPair.QuoteCurrency.Symbol,
                BaseCurrency = alert.TradingPair.BaseCurrency.Symbol,
                QuoteCurrency = alert.TradingPair.QuoteCurrency.Symbol,
                EventType = alert.EventType,
                Direction = alert.Direction,
                TargetPrice = alert.TargetPrice,
                MinimumAmount = alert.MinimumAmount,
                IsActive = alert.IsActive,
                CreatedDate = alert.CreatedDate,
                TriggeredDate = alert.TriggeredDate,
                IsAcknowledged = alert.IsAcknowledged,
                AcknowledgedDate = alert.AcknowledgedDate
            };
        }
    }
}
