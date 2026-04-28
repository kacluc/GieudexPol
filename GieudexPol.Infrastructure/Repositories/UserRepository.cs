using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public async Task<User> GetByUsernameAsync(string username)
        {
            return await Task.FromResult(_data.FirstOrDefault(u => u.Username == username));
        }
    }
}