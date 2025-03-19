using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Domain.DTOs
{
    public class ProfileUpdateDTO
    {
        public short? AccountID { get; set; }
        [Required]
        [Display(Name = "Full Name")]
        public string AccountName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string AccountEmail { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
