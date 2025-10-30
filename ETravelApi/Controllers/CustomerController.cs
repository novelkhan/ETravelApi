using ETravelApi.DTOs.Customer;
using ETravelApi.Interfaces;
using ETravelApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ETravelApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserService _userService;

        public CustomerController(UserManager<User> userManager, IUserService userService, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _userService = userService;
            _signInManager = signInManager;
        }



        [HttpGet("get-customers")]
        public IActionResult Customers()
        {
            return Ok(new JsonResult(new { message = "Only authorized users can view this action method" }));
        }





        // AccountController.cs এ এই methods যোগ করুন

        
        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var profile = new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                dateCreated = user.DateCreated,
                emailConfirmed = user.EmailConfirmed
            };

            return Ok(profile);
        }

        


        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto model)
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            user.FirstName = model.FirstName.ToLower();
            user.LastName = model.LastName.ToLower();
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new JsonResult(new { title = "Profile Updated", message = "Your profile has been updated successfully" }));
        }

        

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // Verify current password
            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, model.CurrentPassword, false);

            if (!passwordCheck.Succeeded)
            {
                return BadRequest("Current password is incorrect");
            }

            // Change password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new JsonResult(new { title = "Password Changed", message = "Your password has been changed successfully" }));
        }











        private User? GetCurrentUser()
        {
            // Get the user ID of the currently logged-in user synchronously
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return null;
            }

            // Retrieve the AppUser object from the database synchronously
            User? user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

            return user;
        }


        private async Task<User?> GetCurrentUserAsync()
        {
            //// Get the user ID of the currently logged-in user
            //string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            // Asynchronously get the user ID of the currently logged-in user
            string? userId = await Task.Run(() => User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userId == null)
            {
                return null;
            }

            // Retrieve the AppUser object from the database
            User? user = await _userManager.FindByIdAsync(userId);

            // Now you have access to the custom properties like FirstName, LastName, etc.
            // ...


            return user;
        }





        private string? GetCurrentUserId()
        {
            // Get the user ID of the currently logged-in user
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return userId;
        }


        private async Task<string?> GetCurrentUserIdAsync()
        {
            //// Get the user ID of the currently logged-in user
            //string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            // Asynchronously get the user ID of the currently logged-in user
            string? userId = await Task.Run(() => User.FindFirstValue(ClaimTypes.NameIdentifier));

            return userId;
        }
    }
}