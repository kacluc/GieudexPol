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
                ThresholdValue = userAlertCreateDto.ThresholdValue,
                PercentageChange = userAlertCreateDto.PercentageChange,
                TimeFrameHours = userAlertCreateDto.TimeFrameHours
            };

            await _userAlertService.CreateUserAlertAsync(userAlert);

            return CreatedAtAction(
                nameof(GetUserAlertsByUserId),
                new { userId = userAlert.UserId },
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

            existingUserAlert.CurrencyId = userAlertUpdateDto.CurrencyId;
            existingUserAlert.AlertType = userAlertUpdateDto.AlertType;
            existingUserAlert.ThresholdValue = userAlertUpdateDto.ThresholdValue;
            existingUserAlert.PercentageChange = userAlertUpdateDto.PercentageChange;
            existingUserAlert.TimeFrameHours = userAlertUpdateDto.TimeFrameHours;
            existingUserAlert.IsActive = userAlertUpdateDto.IsActive;

            await _userAlertService.UpdateUserAlertAsync(existingUserAlert);

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
                CurrencySymbol = userAlert.Currency?.Symbol ?? string.Empty,
                AlertType = userAlert.AlertType,
                ThresholdValue = userAlert.ThresholdValue,
                PercentageChange = userAlert.PercentageChange,
                TimeFrameHours = userAlert.TimeFrameHours,
                IsActive = userAlert.IsActive,
                CreatedDate = userAlert.CreatedDate,
                TriggeredDate = userAlert.TriggeredDate
            };
        }
    }
}
