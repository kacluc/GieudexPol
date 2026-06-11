using GieudexPol.Application.Interfaces;
using GieudexPol.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var authenticatedEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!User.IsInRole(UserRoles.Admin) &&
                !string.Equals(authenticatedEmail, username, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new AdminUserDto
            {
                Id = user.Id,
                Email = user.Username,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Role = user.Role
            });
        }
    }
}
