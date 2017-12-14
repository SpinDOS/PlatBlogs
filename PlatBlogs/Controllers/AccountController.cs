using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlatBlogs.Data;
using PlatBlogs.Pages.Account;
using PlatBlogs.Services;

namespace PlatBlogs.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SettingsModel.PasswordChangeModel passwordChange)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid password change input";
                return RedirectToPage("/Account/Settings");
            }
            var user = await _userManager.GetUserAsync(User);
            var changePasswordResult = await _userManager.ChangePasswordAsync(user,
                passwordChange.OldPassword, passwordChange.Password);

            if (!changePasswordResult.Succeeded)
            {
                TempData["Error"] = changePasswordResult.Errors
                    .First()?.Description ?? 
                    "Password change error occurred";
            }
            else
            {
                TempData["Success"] = "Password changed successfully";
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            return RedirectToPage("/Account/Settings");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToPage("/Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmationLink(string userName, string email)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
                return NotFound();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound();
            if (user.NormalizedEmail == email.ToUpper())
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.EmailConfirmationLink(user.Email, code, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(user.Email, code, callbackUrl);
            }
            TempData["ConfirmationEmailSent"] = user.Email;
            return RedirectToPage("/Account/Login");
        }
    }
}
