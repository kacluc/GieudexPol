using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IUserAlertService _userAlertService;

        public AlertsController(IUserAlertService userAlertService)
        {
            _userAlertService = userAlertService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAlerts(int userId)
        {
            var userAlerts = await _userAlertService.GetUserAlertsByUserIdAsync(userId);
            if (userAlerts == null)
            {
                return NotFound();
            }
            return Ok(userAlerts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlertById(int id)
        {
            var userAlert = await _userAlertService.GetByIdAsync(id);
            if (userAlert == null)
            {
                return NotFound();
            }
            return Ok(userAlert);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAlert([FromBody] UserAlert userAlert)
        {
            await _userAlertService.AddAsync(userAlert);
            return CreatedAtAction(nameof(GetUserAlerts), new { userId = userAlert.UserId }, userAlert);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAlert(int id, [FromBody] UserAlert userAlert)
        {
            if (id != userAlert.Id)
            {
                return BadRequest();
            }
            await _userAlertService.UpdateAsync(userAlert);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAlert(int id)
        {
            var userAlert = await _userAlertService.GetByIdAsync(id);
            if (userAlert == null)
            {
                return NotFound();
            }
            await _userAlertService.DeleteAsync(userAlert);
            return NoContent();
        }
    }
}
