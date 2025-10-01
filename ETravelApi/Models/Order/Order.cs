using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace ETravelApi.Models.Order
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        [Required]
        public string CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public float TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsShipped { get; set; }
        public DateTime? ShippingDate { get; set; }
    }
}