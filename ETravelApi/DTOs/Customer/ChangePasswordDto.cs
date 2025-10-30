using System.ComponentModel.DataAnnotations;

namespace ETravelApi.DTOs.Customer
{
    public class ChangePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Current password must be at least {2}, and maximum {1} characters")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "New password must be at least {2}, and maximum {1} characters")]
        public string NewPassword { get; set; }
    }
}