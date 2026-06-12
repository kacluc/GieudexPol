using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthUser = GieudexPol.Domain.Auth.User;

namespace GieudexPol.Infrastructure.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<AuthUser> _passwordHasher = new();

        public AdminUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(user => user.Username)
                .Select(user => ToDto(user))
                .ToListAsync(cancellationToken);
        }

        public async Task<AdminUserDto?> GetUserAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

            return user == null ? null : ToDto(user);
        }

        public async Task<AdminUserDto> CreateUserAsync(
            CreateAdminUserDto request,
            CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim();
            var role = ValidateAndNormalizeRole(request.Role);

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email jest wymagany.", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Haslo jest wymagane.", nameof(request));
            }

            if (await _context.Users.AnyAsync(
                    user => user.Username.ToLower() == email.ToLower(),
                    cancellationToken))
            {
                throw new InvalidOperationException("Uzytkownik o podanym adresie email juz istnieje.");
            }

            var authId = Guid.NewGuid();
            var authUser = new AuthUser(authId, email, request.Password);
            var user = new User
            {
                AuthId = authId,
                Username = email,
                DisplayName = ResolveDisplayName(email),
                PasswordHash = _passwordHasher.HashPassword(authUser, request.Password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return ToDto(user);
        }

        public async Task<AdminUserDto?> UpdateRoleAsync(
            int id,
            string role,
            CancellationToken cancellationToken = default)
        {
            var normalizedRole = ValidateAndNormalizeRole(role);
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

            if (user == null)
            {
                return null;
            }

            if (string.Equals(user.Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) &&
                normalizedRole != UserRoles.Admin)
            {
                var adminCount = await _context.Users.CountAsync(
                    item => item.Role == UserRoles.Admin,
                    cancellationToken);

                if (adminCount <= 1)
                {
                    throw new InvalidOperationException(
                        "Nie mozna odebrac roli ostatniemu administratorowi.");
                }
            }

            user.Role = normalizedRole;
            await _context.SaveChangesAsync(cancellationToken);

            return ToDto(user);
        }

        public async Task<bool> ResetPasswordAsync(
            int id,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentException("Nowe haslo jest wymagane.", nameof(newPassword));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

            if (user == null)
            {
                return false;
            }

            var authUser = new AuthUser(user.AuthId, user.Username, newPassword, user.Id);
            user.PasswordHash = _passwordHasher.HashPassword(authUser, newPassword);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static string ValidateAndNormalizeRole(string? role)
        {
            if (!UserRoles.IsValid(role))
            {
                throw new ArgumentException("Rola musi miec wartosc Admin albo User.", nameof(role));
            }

            return UserRoles.Normalize(role!);
        }

        private static string ResolveDisplayName(string email)
        {
            var separatorIndex = email.IndexOf('@');
            return separatorIndex > 0 ? email[..separatorIndex] : email;
        }

        private static AdminUserDto ToDto(User user)
        {
            return new AdminUserDto
            {
                Id = user.Id,
                Email = user.Username,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Role = UserRoles.IsValid(user.Role)
                    ? UserRoles.Normalize(user.Role)
                    : UserRoles.User
            };
        }
    }
}
