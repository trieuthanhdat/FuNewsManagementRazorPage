using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.Domain.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterDTO
    {
        public short? AccountID { get; set; } // Nullable for new users

        [Required(ErrorMessage = "Account Name is required.")]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string AccountEmail { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Range(1, 3, ErrorMessage = "Invalid role value.")]
        public int? AccountRole { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }
    }

}
