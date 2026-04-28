using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserService : IService<User>
    {
        Task<User> GetByUsernameAsync(string username);
    }
}