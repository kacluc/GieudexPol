using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/admin/system-accounts")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminSystemAccountsController : ControllerBase
    {
        private readonly IAdminSystemAccountService _systemAccountService;

        public AdminSystemAccountsController(
            IAdminSystemAccountService systemAccountService)
        {
            _systemAccountService = systemAccountService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminSystemAccountDto>>> GetAccounts(
            CancellationToken cancellationToken)
        {
            return Ok(await _systemAccountService.GetAccountsAsync(
                cancellationToken));
        }
    }
}
