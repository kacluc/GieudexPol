using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
    }
}