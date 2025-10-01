using ETravelApi.Data;
using ETravelApi.Models;
using ETravelApi.Models.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public CartController(UserManager<User> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("cart-items")]
    public async Task<IActionResult> CartItems()
    {
        var user = await GetCurrentUserAsync();
        if (user == null || user.CustomerData == null)
        {
            return Unauthorized();
        }

        var cartItems = user.CustomerData.Cart.Select(item => new
        {
            cartItemId = item.CartItemId,
            productName = GetProductName(item.ProductId),
            productPrice = GetProductPrice(item.ProductId),
            productQuantity = item.Quantity,
            productImageSRC = GetProductImage(item.ProductId)
        }).ToList();

        return Ok(cartItems);
    }

    [HttpPost("add-to-cart")]
    public async Task<IActionResult> AddToCart(int packageId)
    {
        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        // Ensure CustomerData exists
        if (user.CustomerData == null)
        {
            user.CustomerData = new CustomerData { UserId = user.Id };
            _context.CustomerData.Add(user.CustomerData);
            await _context.SaveChangesAsync();
        }

        // Ensure Cart is initialized
        if (user.CustomerData.Cart == null)
        {
            user.CustomerData.Cart = new List<CartItem>();
        }

        // Check if the product already exists in the cart
        var existingCartItem = user.CustomerData.Cart
            .FirstOrDefault(item => item.ProductId == packageId);

        if (existingCartItem != null)
        {
            // If product exists, increase the quantity
            existingCartItem.Quantity++;
        }
        else
        {
            // Add new item to cart
            var newCartItem = new CartItem
            {
                ProductId = packageId,
                Quantity = 1,
                CustomerDataId = user.CustomerData.CustomerDataId  // Ensure valid CustomerDataId
            };

            _context.CartItem.Add(newCartItem);
        }

        await _context.SaveChangesAsync();
        return Ok(new { title = "Added to the cart", message = "Item added/updated successfully!" });
    }





    [HttpPost("remove-from-cart")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized();
        }

        var cartItem = _context.CartItem.SingleOrDefault(m => m.CartItemId == cartItemId && m.CustomerDataId == user.CustomerData.CustomerDataId);
        if (cartItem == null)
        {
            return NotFound();
        }

        _context.CartItem.Remove(cartItem);
        await _context.SaveChangesAsync();

        return Ok(new { title = "Item is removed from the cart", message = "The package has been successfully removed from the cart" });
    }

    [HttpGet("incre")]
    public async Task<IActionResult> IncreaseQuantity(int cartItemId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized();
        }

        var cartItem = _context.CartItem.SingleOrDefault(m => m.CartItemId == cartItemId && m.CustomerDataId == user.CustomerData.CustomerDataId);
        if (cartItem == null)
        {
            return NotFound();
        }

        cartItem.Quantity++;
        await _context.SaveChangesAsync();
        return Ok(new { title = "Quantity Increased", message = "Quantity increased successfully" });
    }

    [HttpGet("decre")]
    public async Task<IActionResult> DecreaseQuantity(int cartItemId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized();
        }

        var cartItem = _context.CartItem.SingleOrDefault(m => m.CartItemId == cartItemId && m.CustomerDataId == user.CustomerData.CustomerDataId);
        if (cartItem == null)
        {
            return NotFound();
        }

        if (cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
            await _context.SaveChangesAsync();
            return Ok(new { title = "Quantity decreased", message = "Quantity decreased successfully" });
        }

        return BadRequest(new { title = "Failed to decreased quantity", message = "Quantity cannot be less than 1" });
    }

    [HttpGet("cart-price")]
    public async Task<float> CartPriceAsync()
    {
        float cartPrice = 0;
        var user = await GetCurrentUserAsync();
        if (user == null || user.CustomerData == null)
        {
            return 0;
        }

        foreach (var item in user.CustomerData.Cart)
        {
            cartPrice += GetProductPrice(item.ProductId) * item.Quantity;
        }

        return cartPrice;
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

}