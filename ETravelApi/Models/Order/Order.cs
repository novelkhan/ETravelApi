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
        public string CustomerId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public float TotalAmount { get; set; }

        public bool IsPaid { get; set; }

        // Order Status: Processing, TicketProvided, Completed, Cancelled
        public string OrderStatus { get; set; } = "Processing";

        public bool IsTicketProvided { get; set; } = false;

        public DateTime? TicketProvidedDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        // Keep old properties for backward compatibility
        public bool IsShipped { get; set; } = false;

        public DateTime? ShippingDate { get; set; }

        // Admin Notes
        public string? AdminNotes { get; set; }
    }
}