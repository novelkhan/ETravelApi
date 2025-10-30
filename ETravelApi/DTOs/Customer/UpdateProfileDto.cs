using System.ComponentModel.DataAnnotations;

namespace ETravelApi.DTOs.Customer
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "First name must be at least {2}, and maximum {1} characters")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Last name must be at least {2}, and maximum {1} characters")]
        public string LastName { get; set; }

        [Phone]
        [StringLength(15, MinimumLength = 11, ErrorMessage = "Phone number must be at least {2}, and maximum {1} digits")]
        public string? PhoneNumber { get; set; }
    }
}