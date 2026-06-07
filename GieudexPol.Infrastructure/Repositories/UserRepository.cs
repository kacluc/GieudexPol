using System;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByAuthIdAsync(Guid authId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.AuthId == authId);
        }
    }
}
