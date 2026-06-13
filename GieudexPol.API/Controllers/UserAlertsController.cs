using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAlertsController : ControllerBase
    {
        private readonly IUserAlertService _userAlertService;
        private readonly IUserRepository _userRepository;

        public UserAlertsController(
            IUserAlertService userAlertService,
            IUserRepository userRepository)
        {
            _userAlertService = userAlertService;
            _userRepository = userRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserAlertDto>>> GetUserAlertsByUserId(int userId)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Id != userId)
            {
                return Forbid();
            }

            var userAlerts = await _userAlertService.GetUserAlertsByUserIdAsync(userId);
            var userAlertDtos = userAlerts.Select(MapToDto);
            return Ok(userAlertDtos);
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<UserAlertDto>>> GetMyAlerts()
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var userAlerts = await _userAlertService.GetUserAlertsByUserIdAsync(currentUser.Id);
            return Ok(userAlerts.Select(MapToDto));
        }

        [HttpGet("rate-sources")]
        public async Task<ActionResult<IEnumerable<AlertRateSourceDto>>> GetRateSources()
        {
            var sources = await _userAlertService.GetActiveRateSourcesAsync();
            return Ok(sources.Select(source => new AlertRateSourceDto
            {
                Id = source.Id,
                Code = source.Code,
                Name = source.Name
            }));
        }

        [HttpPost]
        public async Task<ActionResult<UserAlertDto>> CreateUserAlert([FromBody] UserAlertCreateDto userAlertCreateDto)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var userAlert = new UserAlert
            {
                UserId = currentUser.Id,
                CurrencyId = userAlertCreateDto.CurrencyId,
                AlertType = userAlertCreateDto.AlertType,
                PriceSide = userAlertCreateDto.PriceSide,
                ThresholdDirection = userAlertCreateDto.ThresholdDirection,
                RateSourceId = userAlertCreateDto.RateSourceId,
                ThresholdValue = userAlertCreateDto.ThresholdValue,
                PercentageChange = userAlertCreateDto.PercentageChange,
                TimeFrameHours = userAlertCreateDto.TimeFrameHours
            };

            try
            {
                await _userAlertService.CreateUserAlertAsync(userAlert);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }

            return CreatedAtAction(
                nameof(GetMyAlerts),
                routeValues: null,
                MapToDto(userAlert));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAlert(int id, UserAlertUpdateDto userAlertUpdateDto)
        {
            if (id != userAlertUpdateDto.Id)
            {
                return BadRequest();
            }

            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var existingUserAlert = await _userAlertService.GetByIdAsync(id);
            if (existingUserAlert == null)
            {
                return NotFound();
            }

            if (existingUserAlert.UserId != currentUser.Id)
            {
                return Forbid();
            }

            if (userAlertUpdateDto.Status == AlertStatus.Fulfilled &&
                existingUserAlert.Status != AlertStatus.Fulfilled)
            {
                return BadRequest(new
                {
                    message = "Stan Spełniony może ustawić wyłącznie system ewaluacji alertów."
                });
            }

            existingUserAlert.CurrencyId = userAlertUpdateDto.CurrencyId;
            existingUserAlert.AlertType = userAlertUpdateDto.AlertType;
            existingUserAlert.PriceSide = userAlertUpdateDto.PriceSide;
            existingUserAlert.ThresholdDirection = userAlertUpdateDto.ThresholdDirection;
            existingUserAlert.RateSourceId = userAlertUpdateDto.RateSourceId;
            existingUserAlert.ThresholdValue = userAlertUpdateDto.ThresholdValue;
            existingUserAlert.PercentageChange = userAlertUpdateDto.PercentageChange;
            existingUserAlert.TimeFrameHours = userAlertUpdateDto.TimeFrameHours;
            existingUserAlert.Status = userAlertUpdateDto.Status;

            try
            {
                await _userAlertService.UpdateUserAlertAsync(existingUserAlert);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAlert(int id)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var existingUserAlert = await _userAlertService.GetByIdAsync(id);
            if (existingUserAlert == null)
            {
                return NotFound();
            }

            if (existingUserAlert.UserId != currentUser.Id)
            {
                return Forbid();
            }

            await _userAlertService.DeleteUserAlertAsync(id);
            return NoContent();
        }

        private async Task<User?> GetAuthenticatedUserAsync()
        {
            var authIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(authIdValue, out var authId))
            {
                return null;
            }

            return await _userRepository.GetByAuthIdAsync(authId);
        }

        private static UserAlertDto MapToDto(UserAlert userAlert)
        {
            return new UserAlertDto
            {
                Id = userAlert.Id,
                UserId = userAlert.UserId,
                CurrencyId = userAlert.CurrencyId,
                CurrencySymbol = userAlert.Currency?.Symbol ?? string.Empty,
                AlertType = userAlert.AlertType,
                PriceSide = userAlert.PriceSide,
                ThresholdDirection = userAlert.ThresholdDirection,
                RateSourceId = userAlert.RateSourceId,
                RateSourceCode = userAlert.RateSource?.Code,
                RateSourceName = userAlert.RateSource?.Name,
                ThresholdValue = userAlert.ThresholdValue,
                PercentageChange = userAlert.PercentageChange,
                TimeFrameHours = userAlert.TimeFrameHours,
                Status = userAlert.Status,
                CreatedDate = userAlert.CreatedDate,
                TriggeredDate = userAlert.TriggeredDate,
                Logs = userAlert.Logs
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
