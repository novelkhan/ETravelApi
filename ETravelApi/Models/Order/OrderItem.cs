using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ETravelApi.Models.Order
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public byte[]? ProductImage { get; set; }
        public int Quantity { get; set; }
        public float PerUnitPrice { get; set; }
        public float TotalPrice { get; set; }


        [Required]
        [ForeignKey("OrderId")]
        public int OrderId { get; set; }
    }
}