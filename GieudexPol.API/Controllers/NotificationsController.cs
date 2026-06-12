using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;

        public NotificationsController(
            INotificationService notificationService,
            IUserRepository userRepository)
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications()
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(currentUser.Id);
            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                CreatedDate = n.CreatedDate,
                IsRead = n.IsRead
            });
            return Ok(notificationDtos);
        }

        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var updated = await _notificationService.MarkNotificationAsReadAsync(
                id,
                currentUser.Id);
            return updated ? NoContent() : NotFound();
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
    }
}
