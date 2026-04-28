using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public async Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId)
        {
            return await Task.FromResult(_data.Where(w => w.User.Id == userId).ToList());
        }
    }
}