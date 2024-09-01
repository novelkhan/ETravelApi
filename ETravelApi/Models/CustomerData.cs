using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ETravelApi.Models
{
    public class CustomerData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerDataId { get; set; }

        public List<CustomerFile> CustomerFiles { get; set; } = new List<CustomerFile>();

        [Required]
        [ForeignKey("Id")]
        public string UserId { get; set; }
    }
}
