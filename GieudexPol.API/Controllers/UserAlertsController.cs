using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAlertsController : ControllerBase
    {
        private readonly IUserAlertService _userAlertService;

        public UserAlertsController(IUserAlertService userAlertService)
        {
            _userAlertService = userAlertService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserAlertDto>>> GetUserAlertsByUserId(int userId)
        {
            var userAlerts = await _userAlertService.GetUserAlertsByUserIdAsync(userId);
            var userAlertDtos = userAlerts.Select(ua => new UserAlertDto
            {
                Id = ua.Id,
                UserId = ua.UserId,
                CurrencySymbol = ua.Currency.Symbol,
                AlertType = ua.AlertType,
                ThresholdValue = ua.ThresholdValue,
                PercentageChange = ua.PercentageChange,
                TimeFrameHours = ua.TimeFrameHours,
                IsActive = ua.IsActive,
                CreatedDate = ua.CreatedDate,
                TriggeredDate = ua.TriggeredDate
            });
            return Ok(userAlertDtos);
        }

        [HttpPost]
        public async Task<ActionResult<UserAlertDto>> CreateUserAlert(UserAlertCreateDto userAlertCreateDto)
        {
            var userAlert = new UserAlert
            {
                UserId = userAlertCreateDto.UserId,
                CurrencyId = userAlertCreateDto.CurrencyId,
                AlertType = userAlertCreateDto.AlertType,
                ThresholdValue = userAlertCreateDto.ThresholdValue,
                PercentageChange = userAlertCreateDto.PercentageChange,
                TimeFrameHours = userAlertCreateDto.TimeFrameHours
            };

            await _userAlertService.CreateUserAlertAsync(userAlert);

            var createdUserAlertDto = new UserAlertDto
            {
                Id = userAlert.Id,
                UserId = userAlert.UserId,
                // CurrencySymbol will be populated by joining with Currency table in service/repository if needed
                AlertType = userAlert.AlertType,
                ThresholdValue = userAlert.ThresholdValue,
                PercentageChange = userAlert.PercentageChange,
                TimeFrameHours = userAlert.TimeFrameHours,
                IsActive = userAlert.IsActive,
                CreatedDate = userAlert.CreatedDate,
                TriggeredDate = userAlert.TriggeredDate
            };

            return CreatedAtAction(nameof(GetUserAlertsByUserId), new { userId = userAlert.UserId }, createdUserAlertDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAlert(int id, UserAlertUpdateDto userAlertUpdateDto)
        {
            if (id != userAlertUpdateDto.Id)
            {
                return BadRequest();
            }

            var existingUserAlert = await _userAlertService.GetByIdAsync(id);
            if (existingUserAlert == null)
            {
                return NotFound();
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
            await _userAlertService.DeleteUserAlertAsync(id);
            return NoContent();
        }
    }
}
