using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Domain.DTOs
{
    public class LoginDTO
    {
        [Required, EmailAddress]
        public string AccountEmail { get; set; }

        [Required]
        public string Password { get; set; } // Plain text password entered by user
    }

}
