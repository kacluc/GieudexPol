using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ApplicationDbContext _context;

        public SystemAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetPlatformTreasuryAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Users.SingleOrDefaultAsync(
                       user => user.AccountType == AccountType.PlatformTreasury,
                       cancellationToken)
                   ?? throw new InvalidOperationException(
                       "Konto PlatformTreasury nie zostalo skonfigurowane.");
        }

        public async Task<Wallet> GetOrCreateWalletAsync(
            int userId,
            int currencyId,
            CancellationToken cancellationToken = default)
        {
            var tracked = _context.Wallets.Local.FirstOrDefault(wallet =>
                wallet.UserId == userId && wallet.CurrencyId == currencyId);
            if (tracked != null)
            {
                return tracked;
            }

            var wallet = await _context.Wallets.SingleOrDefaultAsync(
                item => item.UserId == userId && item.CurrencyId == currencyId,
                cancellationToken);
            if (wallet != null)
            {
                return wallet;
            }

            wallet = new Wallet
            {
                UserId = userId,
                CurrencyId = currencyId,
                Balance = 0m,
                ReservedBalance = 0m
            };
            await _context.Wallets.AddAsync(wallet, cancellationToken);
            return wallet;
        }
    }
}
