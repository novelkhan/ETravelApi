using System.ComponentModel.DataAnnotations;

namespace ETravelApi.DTOs.Admin
{
    public class MemberAddEditDto
    {
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Phone]
        [StringLength(15, MinimumLength = 11, ErrorMessage = "Phone number must be at least {2}, and maximum {1} digits")]
        public string? PhoneNumber { get; set; }  // New field

        public string Password { get; set; }

        [Required]
        // eg: "Admin,Player,Manager"
        public string Roles { get; set; }
    }
}