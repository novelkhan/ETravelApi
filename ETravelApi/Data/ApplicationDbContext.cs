using ETravelApi.Models;
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
        //public DbSet<UserData> UserData { get; set; }
        //public DbSet<UserFile> UserFiles { get; set; }
    }
}
