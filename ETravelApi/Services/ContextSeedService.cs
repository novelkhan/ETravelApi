﻿using ETravelApi.Models;
using ETravelApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ETravelApi.Services
{
    public class ContextSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ContextSeedService(ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeContextAsync()
        {
            if (_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Count() > 0)
            {
                // applies any pending migration into our database
                await _context.Database.MigrateAsync();
            }

            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.ManagerRole });
                //await _roleManager.CreateAsync(new IdentityRole { Name = SD.PlayerRole });

                await _roleManager.CreateAsync(new IdentityRole { Name = SD.UserRole });
            }

            if (!_userManager.Users.AnyAsync().GetAwaiter().GetResult())
            {
                var admin = new User
                {
                    //FirstName = "Novel",
                    //LastName = "Khan",
                    FirstName = SD.AdminUserFirstName,
                    LastName = SD.AdminUserLastName,
                    UserName = SD.AdminUserName,
                    Email = SD.AdminUserName,
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin, SD.AdminPassword);
                await _userManager.AddToRolesAsync(admin, new[] { SD.AdminRole, SD.ManagerRole, SD.UserRole });
                await _userManager.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Surname, admin.LastName)
                });

                //var manager = new User
                //{
                //    FirstName = "manager",
                //    LastName = "wilson",
                //    UserName = "manager@example.com",
                //    Email = "manager@example.com",
                //    EmailConfirmed = true
                //};
                //await _userManager.CreateAsync(manager, "123456");
                //await _userManager.AddToRoleAsync(manager, SD.ManagerRole);
                //await _userManager.AddClaimsAsync(manager, new Claim[]
                //{
                //    new Claim(ClaimTypes.Email, manager.Email),
                //    new Claim(ClaimTypes.Surname, manager.LastName)
                //});

                //var player = new User
                //{
                //    FirstName = "player",
                //    LastName = "miller",
                //    UserName = "player@example.com",
                //    Email = "player@example.com",
                //    EmailConfirmed = true
                //};
                //await _userManager.CreateAsync(player, "123456");
                //await _userManager.AddToRoleAsync(player, SD.PlayerRole);
                //await _userManager.AddClaimsAsync(player, new Claim[]
                //{
                //    new Claim(ClaimTypes.Email, player.Email),
                //    new Claim(ClaimTypes.Surname, player.LastName)
                //});

                //var vipplayer = new User
                //{
                //    FirstName = "vipplayer",
                //    LastName = "tomson",
                //    UserName = "vipplayer@example.com",
                //    Email = "vipplayer@example.com",
                //    EmailConfirmed = true
                //};
                //await _userManager.CreateAsync(vipplayer, "123456");
                //await _userManager.AddToRoleAsync(vipplayer, SD.PlayerRole);
                //await _userManager.AddClaimsAsync(vipplayer, new Claim[]
                //{
                //    new Claim(ClaimTypes.Email, vipplayer.Email),
                //    new Claim(ClaimTypes.Surname, vipplayer.LastName)
                //});
            }
        }
    }
}