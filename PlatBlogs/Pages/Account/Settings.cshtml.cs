using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PlatBlogs.Attributes;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Pages.Account
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        public SettingsModel(
            IHostingEnvironment environment,
            ApplicationDbContext dbContext)
        {
            _environment = environment;
            _dbContext = dbContext;
        }

        private IHostingEnvironment _environment;
        private ApplicationDbContext _dbContext;

        [BindProperty]
        public RegisterModel.InputModel Input { get; set; }

        void CreateInputFromUser(ApplicationUser user)
        {
            var fullnameParts = user.FullName.Split();
            this.Input = new RegisterModel.InputModel()
            {
                Nickname = user.UserName,
                Email = user.Email,
                Name = fullnameParts[0],
                Surname = fullnameParts[1],
                DateOfBirth = user.DateOfBirth,
                City = user.City,
                Info = user.ShortInfo,
                PublicProfile = user.PublicProfile,
            };
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _dbContext.ApplicationUser.FirstAsync(u =>
                u.NormalizedUserName == User.Identity.Name.ToUpper());
            CreateInputFromUser(user);
            return Page();
        }

        private void IgnoreInputErrors()
        {
            ModelState.Remove("Nickname");
            ModelState.Remove("Email");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            ModelState.Remove("Input.Nickname");
            ModelState.Remove("Input.Email");
            ModelState.Remove("Input.Password");
            ModelState.Remove("Input.ConfirmPassword");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            IgnoreInputErrors();
                
            if (!ModelState.IsValid)
                return Page();

            var user = await _dbContext.ApplicationUser.FirstAsync(u =>
                u.NormalizedUserName == User.Identity.Name.ToUpper());
            
            user.FullName = $"{Input.Name} {Input.Surname}";
            user.DateOfBirth = Input.DateOfBirth;
            user.City = Input.City;
            user.ShortInfo = Input.Info;
            user.PublicProfile = Input.PublicProfile;

            if (Input.Avatar != null)
            {
                if (!string.IsNullOrWhiteSpace(user.AvatarPath))
                {
                    System.IO.File.Delete(user.AvatarFilePath(_environment));
                }
                user.AvatarPath = $"/avatars/{Input.Nickname}{Path.GetExtension(Input.Avatar.FileName)}";
                using (var file = System.IO.File.Open(user.AvatarFilePath(_environment), FileMode.Create))
                {
                    await Input.Avatar.CopyToAsync(file);
                }
            }
            await _dbContext.SaveChangesAsync();

            Input.Nickname = user.UserName;
            Input.Email = user.Email;

            TempData["Success"] = "Settings saved successfully";
            return Page();
        }

        public class PasswordChangeModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Old password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public PasswordChangeModel PasswordChange { get; set; }
    }
}