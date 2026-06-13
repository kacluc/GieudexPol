using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class AdminSystemAccountService : IAdminSystemAccountService
    {
        private readonly ApplicationDbContext _context;

        public AdminSystemAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AdminSystemAccountDto>> GetAccountsAsync(
            CancellationToken cancellationToken = default)
        {
            var accounts = await _context.Users
                .AsNoTracking()
                .Include(user => user.Wallets)
                    .ThenInclude(wallet => wallet.Currency)
                .Where(user =>
                    user.AccountType == AccountType.RateSourceSystem ||
                    user.AccountType == AccountType.PlatformTreasury)
                .OrderBy(user => user.AccountType)
                .ThenBy(user => user.Username)
                .ToListAsync(cancellationToken);

            var sourceBySystemUserId = await _context.RateSources
                .AsNoTracking()
                .Where(source => source.SystemUserId.HasValue)
                .ToDictionaryAsync(
                    source => source.SystemUserId!.Value,
                    cancellationToken);

            return accounts.Select(account =>
            {
                sourceBySystemUserId.TryGetValue(account.Id, out var source);
                return new AdminSystemAccountDto
                {
                    UserId = account.Id,
                    Username = account.Username,
                    DisplayName = account.DisplayName,
                    AccountType = account.AccountType.ToString(),
                    RateSourceCode = source?.Code,
                    RateSourceName = source?.Name,
                    RateSourceIsActive = source?.IsActive,
                    Wallets = account.Wallets
                        .OrderBy(wallet => wallet.Currency.Symbol)
                        .Select(wallet => new AdminSystemWalletDto
                        {
                            CurrencyId = wallet.CurrencyId,
                            CurrencyCode = wallet.Currency.Symbol,
                            CurrencyName = wallet.Currency.Name,
                            Balance = wallet.Balance,
                            ReservedBalance = wallet.ReservedBalance,
                            AvailableBalance = wallet.AvailableBalance
                        })
                        .ToList()
                };
            }).ToList();
        }
    }
}
