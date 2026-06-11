using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IAdminUserService
    {
        Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
        Task<AdminUserDto?> GetUserAsync(int id, CancellationToken cancellationToken = default);
        Task<AdminUserDto> CreateUserAsync(
            CreateAdminUserDto request,
            CancellationToken cancellationToken = default);
        Task<AdminUserDto?> UpdateRoleAsync(
            int id,
            string role,
            CancellationToken cancellationToken = default);
        Task<bool> ResetPasswordAsync(
            int id,
            string newPassword,
            CancellationToken cancellationToken = default);
    }
}
