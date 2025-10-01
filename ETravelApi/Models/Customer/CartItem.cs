using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ETravelApi.Models.Customer
{
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }


        [Required]
        [ForeignKey("CustomerDataId")]
        public int CustomerDataId { get; set; }
    }
}