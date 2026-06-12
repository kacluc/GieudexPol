using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IRateSourceRepository : IRepository<RateSource>
    {
        Task<RateSource?> GetByCodeAsync(string code);
        Task<IReadOnlyList<RateSource>> GetActiveAsync();
    }
}
