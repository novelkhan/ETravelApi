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
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ETravelApi.Data;

namespace ETravelApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public OrderController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        //[HttpPost("cart-checkout")]
        //public async Task<IActionResult> CartCheckout(int[] selectedCartItemsId)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (user == null || user.CustomerData == null)
        //    {
        //        return Unauthorized();
        //    }

        //    var selectedCartItems = user.CustomerData.Cart.Where(item => selectedCartItemsId.Contains(item.CartItemId)).ToList();
        //    var order = new Order
        //    {
        //        CustomerId = user.Id,
        //        OrderDate = DateTime.UtcNow,
        //        OrderItems = selectedCartItems.Select(item => new OrderItem
        //        {
        //            ProductId = item.ProductId,
        //            ProductName = GetProductName(item.ProductId),
        //            ProductImage = GetProductImage(item.ProductId),
        //            Quantity = item.Quantity,
        //            PerUnitPrice = GetProductPrice(item.ProductId),
        //            TotalPrice = item.Quantity * GetProductPrice(item.ProductId)
        //        }).ToList(),
        //        TotalAmount = selectedCartItems.Sum(item => item.Quantity * GetProductPrice(item.ProductId)),
        //        IsPaid = true,
        //        IsShipped = false,
        //        ShippingDate = null
        //    };
        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();
        //    foreach (var cartItemId in selectedCartItemsId)
        //    {
        //        RemoveFromCart(cartItemId);
        //    }
        //    return Ok(new { message = "Order placed successfully." });
        //}



        //https://chat.deepseek.com/a/chat/s/320a9b23-f159-4589-a616-e1db1dff6476
        //https://chatgpt.com/c/67cdc0e8-1950-800b-9395-f176b7d8241f
        [HttpPost("cart-checkout")]
        public async Task<IActionResult> CartCheckout([FromBody] int[] selectedCartItemsId)
        {       //Payment guide: https://chatgpt.com/c/67c21133-2b44-8002-88a4-7576bdcc3ad1 
            var user = await GetCurrentUserAsync();
            if (user == null || user.CustomerData == null)
            {
                return Unauthorized();
            }

            var selectedCartItems = user.CustomerData.Cart
                .Where(item => selectedCartItemsId.Contains(item.CartItemId))
                .ToList();

            if (!selectedCartItems.Any())
            {
                return BadRequest("No items selected for checkout.");
            }

            var order = new Order
            {
                CustomerId = user.Id,
                OrderDate = DateTime.UtcNow.AddHours(6),
                OrderItems = new List<OrderItem>(),
                TotalAmount = 0,
                IsPaid = true,
                IsShipped = false
            };

            foreach (var item in selectedCartItems)
            {
                var product = await _context.Packages.Include(pd => pd.PackageData).ThenInclude(pi => pi.PackageImages).FirstOrDefaultAsync(p => p.PackageId == item.ProductId);
                if (product == null) continue;

                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.PackageName,
                    ProductImage = product.PackageData?.PackageImages?.FirstOrDefault()?.filebytes,
                    Quantity = item.Quantity,
                    PerUnitPrice = product.Price,
                    TotalPrice = item.Quantity * product.Price
                };

                order.OrderItems.Add(orderItem);
                order.TotalAmount += orderItem.TotalPrice;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            //await RemoveFromCart(selectedCartItemsId);
            foreach (var cartItemId in selectedCartItemsId)
            {
                await RemoveFromCart(cartItemId);
            }

            return Ok(new { message = "Order placed successfully.", orderId = order.OrderId });
        } // https://chat.deepseek.com/a/chat/s/f517a99e-b3dc-469f-ab9d-55a89028ab42



        //[HttpPost("single-checkout")]
        //public async Task<IActionResult> SingleCheckout(int packageId)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (user == null)
        //    {
        //        return Unauthorized();
        //    }
        //    var order = new Order
        //    {
        //        CustomerId = user.Id,
        //        OrderDate = DateTime.UtcNow,
        //        OrderItems = new[]
        //        {
        //            new OrderItem
        //            {
        //                ProductId = packageId,
        //                ProductName = GetProductName(packageId),
        //                ProductImage = GetProductImage(packageId),
        //                Quantity = 1,
        //                PerUnitPrice = GetProductPrice(packageId),
        //                TotalPrice = GetProductPrice(packageId)
        //            }
        //        }.ToList(),
        //        TotalAmount = GetProductPrice(packageId),
        //        IsPaid = true,
        //        IsShipped = false,
        //        ShippingDate = null
        //    };
        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Order placed successfully." });
        //}


        [HttpPost("single-checkout")]
        public async Task<IActionResult> SingleCheckout([FromBody] int packageId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var product = await _context.Packages.Include(pd => pd.PackageData).ThenInclude(pi => pi.PackageImages).FirstOrDefaultAsync(p => p.PackageId == packageId);
            if (product == null)
            {
                return NotFound("Package not found.");
            }

            var order = new Order
            {
                CustomerId = user.Id,
                OrderDate = DateTime.UtcNow.AddHours(6),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = packageId,
                        ProductName = product.PackageName,
                        ProductImage = product.PackageData?.PackageImages?.FirstOrDefault()?.filebytes,
                        Quantity = 1,
                        PerUnitPrice = product.Price,
                        TotalPrice = product.Price
                    }
                },
                TotalAmount = product.Price,
                IsPaid = true,
                IsShipped = false
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order placed successfully.", orderId = order.OrderId });
        }




        //[HttpPost("order-details")]
        //public async Task<IActionResult> OrderDetails([FromBody] OrderDetailsRequest request)
        //{
        //    if (request == null || request.OrderId <= 0)
        //    {
        //        return BadRequest("Invalid order ID.");
        //    }

        //    var user = await GetCurrentUserAsync();
        //    if (user == null)
        //    {
        //        return Unauthorized();
        //    }

        //    var order = _context.Orders
        //        .Include(o => o.OrderItems)
        //        .FirstOrDefault(o => o.OrderId == request.OrderId && o.CustomerId == user.Id);
        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    var orderDetails = new
        //    {
        //        orderId = order.OrderId,
        //        orderDate = order.OrderDate,
        //        totalAmount = order.TotalAmount,
        //        isPaid = order.IsPaid,
        //        isShipped = order.IsShipped,
        //        shippingDate = order.ShippingDate,
        //        orderItems = order.OrderItems.Select(oi => new
        //        {
        //            productId = oi.ProductId,
        //            productName = oi.ProductName,
        //            productImage = oi.ProductImage,
        //            quantity = oi.Quantity,
        //            perUnitPrice = oi.PerUnitPrice,
        //            totalPrice = oi.TotalPrice
        //        }).ToList()
        //    };
        //    return Ok(orderDetails);
        //}

        //public class OrderDetailsRequest
        //{
        //    public int OrderId { get; set; }
        //}



        [HttpGet("order-details/{orderId}")]
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == user.Id);
            if (order == null)
            {
                return NotFound();
            }
            var orderDetails = new
            {
                orderId = order.OrderId,
                orderDate = order.OrderDate,
                totalAmount = order.TotalAmount,
                isPaid = order.IsPaid,
                isShipped = order.IsShipped,
                shippingDate = order.ShippingDate,
                orderItems = order.OrderItems.Select(oi => new
                {
                    productId = oi.ProductId,
                    productName = oi.ProductName,
                    productImage = oi.ProductImage,
                    quantity = oi.Quantity,
                    perUnitPrice = oi.PerUnitPrice,
                    totalPrice = oi.TotalPrice
                }).ToList()
            };
            return Ok(orderDetails);
        }



        [HttpPost("order-history")]
        public async Task<IActionResult> OrderHistory()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            var orders = _context.Orders
                .Where(o => o.CustomerId == user.Id)
                .Select(o => new
                {
                    orderId = o.OrderId,
                    orderDate = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    isPaid = o.IsPaid,
                    isShipped = o.IsShipped,
                    shippingDate = o.ShippingDate,
                    orderItems = o.OrderItems.Select(oi => new
                    {
                        productId = oi.ProductId,
                        productName = oi.ProductName,
                        productImage = oi.ProductImage,
                        quantity = oi.Quantity,
                        perUnitPrice = oi.PerUnitPrice,
                        totalPrice = oi.TotalPrice
                    }).ToList()
                }).ToList();
            return Ok(orders);
        }



        private async Task<User> GetCurrentUserAsync()
        {
            #pragma warning disable CS8603 // Possible null reference return.
            return await _userManager.Users
                .Include(u => u.CustomerData) // Ensure CustomerData is loaded
                .ThenInclude(cd => cd.Cart)   // Ensure Cart is loaded
                .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            #pragma warning restore CS8603 // Possible null reference return.
        }

        private async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                // Validate input
                if (cartItemId <= 0)
                {
                    return BadRequest("Invalid cart item ID.");
                }

                // Get current user
                var user = await GetCurrentUserAsync();
                if (user == null || user.CustomerData == null)
                {
                    return Unauthorized("User not found or customer data is missing.");
                }

                // Find the cart item
                var cartItem = await _context.CartItem
                    .SingleOrDefaultAsync(m => m.CartItemId == cartItemId && m.CustomerDataId == user.CustomerData.CustomerDataId);

                if (cartItem == null)
                {
                    return NotFound("Cart item not found.");
                }

                // Remove the cart item
                _context.CartItem.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cart item removed successfully." });
            }
            catch (Exception ex)
            {
                // Log the error (optional: you can log it to a file or database if needed)
                // For now, we are returning a generic error message
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        private float GetProductPrice(int packageId)
        {
            var package = _context.Packages.AsNoTracking().FirstOrDefault(m => m.PackageId == packageId);
            return package?.Price ?? 0;
        }

        private string GetProductName(int packageId)
        {
            var package = _context.Packages.AsNoTracking().FirstOrDefault(m => m.PackageId == packageId);
            return package?.PackageName ?? "";
        }

        private byte[] GetProductImage(int packageId)
        {
            //var package = _context.Packages.Include(pd => pd.PackageData)
            //    .AsNoTracking()
            //    .FirstOrDefault(m => m.PackageId == packageId);
            var package = _context.Packages.Include(pd => pd.PackageData) // Include package details
                    .ThenInclude(pd => pd.PackageImages) // Include associated images
                    .FirstOrDefault(p => p.PackageId == packageId);

            if (package == null)
            {
                Console.WriteLine($"Package with ID {packageId} not found.");
                return null;
            }

            if (package.PackageData == null)
            {
                Console.WriteLine($"PackageData is null for package ID {packageId}");
                return null;
            }

            if (package.PackageData.PackageImages == null || !package.PackageData.PackageImages.Any())
            {
                Console.WriteLine($"No images found for package ID {packageId}");
                return null;
            }

            return package.PackageData.PackageImages.FirstOrDefault()?.filebytes;
        }








        ///////////////////////////Codes for bKash payment start//////////////////////////////
        ///                 Just replace the previous CartCkeckout method                  ///
        ///////////////////////////Codes for bKash payment start//////////////////////////////


        //[HttpPost("cart-checkout")]
        //public async Task<IActionResult> CartCheckout([FromBody] int[] selectedCartItemsId)
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (user == null || user.CustomerData == null)
        //    {
        //        return Unauthorized();
        //    }

        //    var selectedCartItems = user.CustomerData.Cart
        //        .Where(item => selectedCartItemsId.Contains(item.CartItemId))
        //        .ToList();

        //    if (!selectedCartItems.Any())
        //    {
        //        return BadRequest("No items selected for checkout.");
        //    }

        //    // Calculate total amount
        //    decimal totalAmount = 0;
        //    foreach (var item in selectedCartItems)
        //    {
        //        var product = await _context.Packages.FirstOrDefaultAsync(p => p.PackageId == item.ProductId);
        //        if (product == null) continue;
        //        totalAmount += item.Quantity * product.Price;
        //    }

        //    // Step 1: Create a Bkash payment request
        //    var bkashPaymentResponse = await CreateBkashPayment(totalAmount);
        //    if (!bkashPaymentResponse.IsSuccess)
        //    {
        //        return BadRequest("Failed to initiate Bkash payment.");
        //    }

        //    // Step 2: Redirect the user to Bkash payment page (frontend will handle this)
        //    return Ok(new
        //    {
        //        paymentUrl = bkashPaymentResponse.PaymentUrl,
        //        message = "Redirect to Bkash payment page."
        //    });
        //}

        //private async Task<BkashPaymentResponse> CreateBkashPayment(decimal amount)
        //{
        //    // Replace with your Bkash API credentials
        //    var appKey = "your_app_key";
        //    var appSecret = "your_app_secret";
        //    var username = "your_username";
        //    var password = "your_password";

        //    // Step 1: Get Bkash token
        //    var tokenResponse = await GetBkashToken(appKey, appSecret, username, password);
        //    if (string.IsNullOrEmpty(tokenResponse?.IdToken))
        //    {
        //        return new BkashPaymentResponse { IsSuccess = false, Message = "Failed to get Bkash token." };
        //    }

        //    // Step 2: Create payment request
        //    var paymentRequest = new
        //    {
        //        mode = "0011", // Payment mode
        //        payerReference = "user_id", // Replace with user ID or reference
        //        callbackURL = "https://yourdomain.com/api/order/bkash-callback", // Callback URL
        //        amount = amount,
        //        currency = "BDT",
        //        intent = "sale",
        //        merchantInvoiceNumber = Guid.NewGuid().ToString() // Unique invoice number
        //    };

        //    using (var httpClient = new HttpClient())
        //    {
        //        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.IdToken);
        //        var response = await httpClient.PostAsJsonAsync("https://api.bkash.com/v1.2.0-beta/checkout/payment/create", paymentRequest);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return new BkashPaymentResponse { IsSuccess = false, Message = "Failed to create Bkash payment." };
        //        }

        //        var paymentResponse = await response.Content.ReadFromJsonAsync<BkashPaymentResponse>();
        //        return paymentResponse;
        //    }
        //}

        //private async Task<BkashTokenResponse> GetBkashToken(string appKey, string appSecret, string username, string password)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        //        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

        //        var tokenRequest = new
        //        {
        //            app_key = appKey,
        //            app_secret = appSecret
        //        };

        //        var response = await httpClient.PostAsJsonAsync("https://api.bkash.com/v1.2.0-beta/checkout/token/grant", tokenRequest);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return null;
        //        }

        //        return await response.Content.ReadFromJsonAsync<BkashTokenResponse>();
        //    }
        //}

        //public class BkashTokenResponse
        //{
        //    public string IdToken { get; set; }
        //}

        //public class BkashPaymentResponse
        //{
        //    public bool IsSuccess { get; set; }
        //    public string PaymentUrl { get; set; }
        //    public string Message { get; set; }
        //}






        ////CallBack
        //[HttpPost("bkash-callback")]
        //public async Task<IActionResult> BkashCallback([FromBody] BkashCallbackModel callbackData)
        //{
        //    // Verify the payment status
        //    if (callbackData.Status != "success")
        //    {
        //        return BadRequest("Payment failed.");
        //    }

        //    // Step 1: Execute the payment
        //    var executeResponse = await ExecuteBkashPayment(callbackData.PaymentID);
        //    if (!executeResponse.IsSuccess)
        //    {
        //        return BadRequest("Failed to execute payment.");
        //    }

        //    // Step 2: Create the order
        //    var user = await GetCurrentUserAsync();
        //    if (user == null || user.CustomerData == null)
        //    {
        //        return Unauthorized();
        //    }

        //    var order = new Order
        //    {
        //        CustomerId = user.Id,
        //        OrderDate = DateTime.UtcNow,
        //        TotalAmount = (float)executeResponse.Amount,
        //        IsPaid = true,
        //        IsShipped = false
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Payment successful.", orderId = order.OrderId });
        //}

        //private async Task<BkashExecuteResponse> ExecuteBkashPayment(string paymentId)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        var tokenResponse = await GetBkashToken("your_app_key", "your_app_secret", "your_username", "your_password");
        //        if (string.IsNullOrEmpty(tokenResponse?.IdToken))
        //        {
        //            return new BkashExecuteResponse { IsSuccess = false };
        //        }

        //        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.IdToken);
        //        var response = await httpClient.PostAsJsonAsync("https://api.bkash.com/v1.2.0-beta/checkout/payment/execute", new { paymentID = paymentId });
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return new BkashExecuteResponse { IsSuccess = false };
        //        }

        //        return await response.Content.ReadFromJsonAsync<BkashExecuteResponse>();
        //    }
        //}

        //public class BkashCallbackModel
        //{
        //    public string PaymentID { get; set; }
        //    public string Status { get; set; }
        //}

        //public class BkashExecuteResponse
        //{
        //    public bool IsSuccess { get; set; }
        //    public decimal Amount { get; set; }
        //}



        ///////////////////////////Codes for bKash payment end//////////////////////////////
        ///                 Just replace the previous CartCkeckout method                ///
        ///////////////////////////Codes for bKash payment end//////////////////////////////
    }
}