using System.ComponentModel.DataAnnotations;

namespace GieudexPol.Application.DTOs
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";

        public static bool IsValid(string? role)
        {
            return string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(role, User, StringComparison.OrdinalIgnoreCase);
        }

        public static string Normalize(string role)
        {
            return string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase) ? Admin : User;
        }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class CreateAdminUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = UserRoles.User;
    }

    public class UpdateUserRoleDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class ResetUserPasswordDto
    {
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
