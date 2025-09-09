using System.ComponentModel.DataAnnotations;

namespace Talabat.APIs.Dtos
{
    public class ResetEmailPasswordDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        [RegularExpression(@"^(?=.{6,10}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+}{""':;'?/>\.<,])(?!.*\s).*$",
            ErrorMessage = "Password must have 1 Uppercase, 1 Lowercase, 1 number, 1 non-alphanumeric character, and be 6–10 characters with no spaces.")]
        public string Password { get; set; } = null!;
        [Required]
        public string Token { get; set; } = null!;
    }
}
