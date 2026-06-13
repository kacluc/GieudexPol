using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers(
            CancellationToken cancellationToken)
        {
            return Ok(await _adminUserService.GetUsersAsync(cancellationToken));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminUserDto>> GetUser(
            int id,
            CancellationToken cancellationToken)
        {
            var user = await _adminUserService.GetUserAsync(id, cancellationToken);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<AdminUserDto>> CreateUser(
            [FromBody] CreateAdminUserDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await _adminUserService.CreateUserAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpPut("{id:int}/role")]
        public async Task<ActionResult<AdminUserDto>> UpdateRole(
            int id,
            [FromBody] UpdateUserRoleDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await _adminUserService.UpdateRoleAsync(
                    id,
                    request.Role,
                    cancellationToken);

                return user == null ? NotFound() : Ok(user);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpPut("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(
            int id,
            [FromBody] ResetUserPasswordDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = await _adminUserService.ResetPasswordAsync(
                    id,
                    request.NewPassword,
                    cancellationToken);

                return updated ? NoContent() : NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }
    }
}
