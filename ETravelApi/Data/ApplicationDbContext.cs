using ETravelApi.Models;
using ETravelApi.Models.Customer;
using ETravelApi.Models.Order;
using ETravelApi.Models.Package;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ETravelApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //add-migration AddingUserToDatabase -o Data/Migrations
        }


        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<CustomerData> CustomerData { get; set; }
        public DbSet<CustomerFile> CustomerFiles { get; set; }

        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageData> PackageData { get; set; }
        public DbSet<PackageImage> PackageImages { get; set; }

        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}