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

            if (request.Status == AlertStatus.Fulfilled &&
                alert.Status != AlertStatus.Fulfilled)
            {
                return BadRequest(new
                {
                    message = "Stan Spełniony może ustawić wyłącznie system ewaluacji alertów."
                });
            }

            alert.TradingPairId = request.TradingPairId;
            alert.EventType = request.EventType;
            alert.Direction = request.Direction;
            alert.TargetPrice = request.TargetPrice;
            alert.MinimumAmount = request.MinimumAmount;
            alert.Status = request.Status;

            try
            {
                await _alertService.UpdateAsync(alert, cancellationToken);
                if (alert.Status != AlertStatus.Inactive)
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
                Direction = alert.EventType switch
                {
                    TradingAlertEvent.SellOrder => ThresholdDirection.BelowOrEqual,
                    TradingAlertEvent.BuyOrder => ThresholdDirection.AboveOrEqual,
                    _ => alert.Direction
                },
                TargetPrice = alert.TargetPrice,
                MinimumAmount = alert.MinimumAmount,
                Status = alert.Status,
                CreatedDate = alert.CreatedDate,
                TriggeredDate = alert.TriggeredDate,
                Logs = alert.Logs
                    .OrderByDescending(log => log.CreatedDate)
                    .Select(MapLog)
                    .ToList()
            };
        }

        private static AlertLogDto MapLog(AlertLog log)
        {
            return new AlertLogDto
            {
                Id = log.Id,
                Message = log.Message,
                CreatedDate = log.CreatedDate,
                CurrentPrice = log.CurrentPrice,
                CurrentAmount = log.CurrentAmount,
                SourceSummary = log.SourceSummary,
                EffectiveDate = log.EffectiveDate
            };
        }
    }
}
