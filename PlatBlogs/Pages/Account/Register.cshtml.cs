using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlatBlogs.Attributes;
using PlatBlogs.Data;
using PlatBlogs.Extensions;
using PlatBlogs.Services;

namespace PlatBlogs.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHostingEnvironment _environment;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger,
            IEmailSender emailSender,
            IHostingEnvironment environment)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [DataType(DataType.Text)]
            [Display(Name = "Nickname")]
            [RegularExpression(@"^[a-zA-Z]([-_]*[a-zA-Z\d]+)*$", ErrorMessage = "Invalid nickname.")]
            public string Nickname { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(20, ErrorMessage = "The name must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [DataType(DataType.Text)]
            [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Invalid name.")]
            public string Name { get; set; }

            [Required]
            [StringLength(40, ErrorMessage = "The surname must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [DataType(DataType.Text)]
            [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Invalid surname.")]
            public string Surname { get; set; }

            [DataType(DataType.Date)]
            [DateOfBirthValidation]
            [Display(Name = "Date of birth")]
            public DateTime? DateOfBirth { get; set; }

            [DataType(DataType.Text)]
            [StringLength(30, ErrorMessage = "The city must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            public string City { get; set; }

            [DataType(DataType.Upload)]
            [AvatarValidation]
            public IFormFile Avatar { get; set; }

            [StringLength(140, ErrorMessage = "The info must be shorted than {1} characters.")]
            [DataType(DataType.Text)]
            public string Info { get; set; }
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return LocalRedirect(Url.GetLocalUrl(returnUrl));
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return LocalRedirect(Url.GetLocalUrl(returnUrl));

            ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Nickname,
                    Email = Input.Email,
                    FullName = $"{Input.Name} {Input.Surname}",
                    DateOfBirth = Input.DateOfBirth,
                    City = Input.City,
                    ShortInfo = Input.Info,
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if (Input.Avatar != null)
                    {
                        user.AvatarPath = Path.Combine("/avatars",
                            user.UserName + Path.GetExtension(Input.Avatar.FileName));
                        using (var file = System.IO.File.Open(user.AvatarFilePath(_environment), FileMode.Create))
                        {
                            await Input.Avatar.CopyToAsync(file);
                        }
                    }
                    else
                    {
                        user.AvatarPath = "/avatars/_no_image_.png";
                    }

                    _logger.LogInformation("User created a new account with password.");

                    var id = user.Id;
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(Input.Email, callbackUrl);

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    if (returnUrl == null)
                    {
                        TempData["EmailToConfirm"] = Input.Email;
                        return RedirectToPage("Login");
                    }
                    return LocalRedirect(Url.GetLocalUrl(returnUrl));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
