using ETravelApi.Data;
using ETravelApi.Models;
using ETravelApi.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETravelApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminOrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminOrderController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/AdminOrder/all-orders
        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    orderId = o.OrderId,
                    customerId = o.CustomerId,
                    customerName = o.CustomerName,
                    customerEmail = o.CustomerEmail,
                    customerPhone = o.CustomerPhone,
                    orderDate = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    isPaid = o.IsPaid,
                    orderStatus = o.OrderStatus,
                    isTicketProvided = o.IsTicketProvided,
                    ticketProvidedDate = o.TicketProvidedDate,
                    isCompleted = o.IsCompleted,
                    completedDate = o.CompletedDate,
                    adminNotes = o.AdminNotes,
                    totalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    orderItems = o.OrderItems.Select(oi => new
                    {
                        orderItemId = oi.OrderItemId,
                        productId = oi.ProductId,
                        productName = oi.ProductName,
                        productImage = oi.ProductImage,
                        quantity = oi.Quantity,
                        perUnitPrice = oi.PerUnitPrice,
                        totalPrice = oi.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/AdminOrder/order-details/{orderId}
        [HttpGet("order-details/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderId == orderId)
                .Select(o => new
                {
                    orderId = o.OrderId,
                    customerId = o.CustomerId,
                    customerName = o.CustomerName,
                    customerEmail = o.CustomerEmail,
                    customerPhone = o.CustomerPhone,
                    orderDate = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    isPaid = o.IsPaid,
                    orderStatus = o.OrderStatus,
                    isTicketProvided = o.IsTicketProvided,
                    ticketProvidedDate = o.TicketProvidedDate,
                    isCompleted = o.IsCompleted,
                    completedDate = o.CompletedDate,
                    adminNotes = o.AdminNotes,
                    orderItems = o.OrderItems.Select(oi => new
                    {
                        orderItemId = oi.OrderItemId,
                        productId = oi.ProductId,
                        productName = oi.ProductName,
                        productImage = oi.ProductImage,
                        quantity = oi.Quantity,
                        perUnitPrice = oi.PerUnitPrice,
                        totalPrice = oi.TotalPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(order);
        }

        // PUT: api/AdminOrder/provide-ticket/{orderId}
        [HttpPut("provide-ticket/{orderId}")]
        public async Task<IActionResult> ProvideTicket(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            order.IsTicketProvided = true;
            order.TicketProvidedDate = DateTime.UtcNow.AddHours(6);
            order.OrderStatus = "TicketProvided";

            await _context.SaveChangesAsync();

            return Ok(new { title = "Ticket Provided", message = "Ticket has been provided to the customer successfully!" });
        }

        // PUT: api/AdminOrder/mark-completed/{orderId}
        [HttpPut("mark-completed/{orderId}")]
        public async Task<IActionResult> MarkAsCompleted(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            order.IsCompleted = true;
            order.CompletedDate = DateTime.UtcNow.AddHours(6);
            order.OrderStatus = "Completed";

            await _context.SaveChangesAsync();

            return Ok(new { title = "Order Completed", message = "Order has been marked as completed successfully!" });
        }

        // PUT: api/AdminOrder/cancel-order/{orderId}
        [HttpPut("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            order.OrderStatus = "Cancelled";

            await _context.SaveChangesAsync();

            return Ok(new { title = "Order Cancelled", message = "Order has been cancelled successfully!" });
        }

        // PUT: api/AdminOrder/update-status/{orderId}
        [HttpPut("update-status/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto model)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            order.OrderStatus = model.OrderStatus;

            if (model.OrderStatus == "TicketProvided" && !order.IsTicketProvided)
            {
                order.IsTicketProvided = true;
                order.TicketProvidedDate = DateTime.UtcNow.AddHours(6);
            }

            if (model.OrderStatus == "Completed" && !order.IsCompleted)
            {
                order.IsCompleted = true;
                order.CompletedDate = DateTime.UtcNow.AddHours(6);
            }

            await _context.SaveChangesAsync();

            return Ok(new { title = "Status Updated", message = "Order status has been updated successfully!" });
        }

        // PUT: api/AdminOrder/update-notes/{orderId}
        [HttpPut("update-notes/{orderId}")]
        public async Task<IActionResult> UpdateAdminNotes(int orderId, [FromBody] UpdateAdminNotesDto model)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            order.AdminNotes = model.AdminNotes;

            await _context.SaveChangesAsync();

            return Ok(new { title = "Notes Updated", message = "Admin notes have been updated successfully!" });
        }

        // GET: api/AdminOrder/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetOrderStatistics()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var processingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Processing");
            var ticketProvidedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "TicketProvided");
            var completedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Completed");
            var cancelledOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Cancelled");
            var totalRevenue = await _context.Orders.Where(o => o.IsPaid).SumAsync(o => o.TotalAmount);

            return Ok(new
            {
                totalOrders,
                processingOrders,
                ticketProvidedOrders,
                completedOrders,
                cancelledOrders,
                totalRevenue
            });
        }
    }

    // DTOs
    public class UpdateOrderStatusDto
    {
        public string OrderStatus { get; set; }
    }

    public class UpdateAdminNotesDto
    {
        public string AdminNotes { get; set; }
    }
}