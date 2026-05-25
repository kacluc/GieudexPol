using System.Threading.Tasks;

namespace GieudexPol.Domain.Auth
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
