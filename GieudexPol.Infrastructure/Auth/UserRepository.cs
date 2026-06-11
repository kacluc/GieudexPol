using GieudexPol.Domain.Auth;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthUser = GieudexPol.Domain.Auth.User;
using AppUser = GieudexPol.Domain.Entities.User;

namespace GieudexPol.Infrastructure.Auth
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<AuthUser> _passwordHasher = new();

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuthUser?> GetByEmailAsync(string email)
        {
            var applicationUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Username == email);

            if (applicationUser == null)
            {
                return null;
            }

            return new AuthUser(
                applicationUser.AuthId,
                applicationUser.Username,
                applicationUser.PasswordHash,
                applicationUser.Id,
                applicationUser.Role,
                applicationUser.DisplayName);
        }

        public async Task AddAsync(AuthUser user)
        {
            var existingUser = await _context.Users.AnyAsync(applicationUser => applicationUser.Username == user.Email);
            if (existingUser)
            {
                throw new UserAlreadyExistsException(user.Email);
            }

            var hashedPassword = _passwordHasher.HashPassword(user, user.HashedPassword);
            var baseCurrency = await _context.Currencies
                .SingleOrDefaultAsync(currency =>
                    currency.Symbol == TradingCurrencyCatalog.BaseCurrencySymbol &&
                    currency.IsActive)
                ?? throw new InvalidOperationException(
                    $"Active {TradingCurrencyCatalog.BaseCurrencySymbol} currency is required to register a user.");

            var applicationUser = new AppUser
            {
                AuthId = user.Id,
                Username = user.Email,
                DisplayName = user.DisplayName,
                PasswordHash = hashedPassword,
                Role = "User"
            };

            applicationUser.Wallets.Add(new Wallet
            {
                Currency = baseCurrency,
                Balance = 0m
            });

            await _context.Users.AddAsync(applicationUser);
            await _context.SaveChangesAsync();
            user.AssignApplicationUserId(applicationUser.Id);
            user.UpdatePassword(hashedPassword);
        }

        public async Task UpdateAsync(AuthUser user)
        {
            var applicationUser = await _context.Users.FirstOrDefaultAsync(entity => entity.AuthId == user.Id);
            if (applicationUser == null)
            {
                throw new UserNotFoundException(user.Email);
            }

            applicationUser.Username = user.Email;
            applicationUser.DisplayName = user.DisplayName;
            applicationUser.PasswordHash = user.HashedPassword;

            await _context.SaveChangesAsync();
        }
    }
}
