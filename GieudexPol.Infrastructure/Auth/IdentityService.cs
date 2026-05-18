using GieudexPol.Application.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthUser = GieudexPol.Domain.Auth.User;

namespace GieudexPol.Infrastructure.Auth
{
    public class IdentityService : IIdentityService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<AuthUser> _passwordHasher = new();

        public IdentityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckPasswordAsync(string email, string password)
        {
            var applicationUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Username == email);

            if (applicationUser == null)
            {
                return false;
            }

            var authUser = new AuthUser(applicationUser.AuthId, applicationUser.Username, applicationUser.PasswordHash);
            var result = _passwordHasher.VerifyHashedPassword(authUser, applicationUser.PasswordHash, password);

            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
