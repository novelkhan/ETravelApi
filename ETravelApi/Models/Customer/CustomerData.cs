using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ETravelApi.Models.Customer
{
    public class CustomerData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerDataId { get; set; }

        public List<CustomerFile> CustomerFiles { get; set; } = new List<CustomerFile>();
        public List<CartItem> Cart { get; set; }

        [Required]
        [ForeignKey("Id")]
        public string UserId { get; set; }
    }
}