using System.ComponentModel.DataAnnotations;

namespace GieudexPol.Application.Auth.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@".*\S.*", ErrorMessage = "Display name cannot contain only whitespace.")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
