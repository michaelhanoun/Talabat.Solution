using System.ComponentModel.DataAnnotations;

namespace Talabat.APIs.Dtos
{
    public class EmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
