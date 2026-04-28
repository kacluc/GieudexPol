using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId)
        {
            return await Task.FromResult(_data.Where(t => t.User.Id == userId).ToList());
        }
    }
}