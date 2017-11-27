﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Pages
{
    public class UserModel : PageModel
    {
        public UserModel(UserManager<ApplicationUser> userManager) { UserManager = userManager; }

        public UserManager<ApplicationUser> UserManager { get; set; }

        public IEnumerable<Post> Posts { get; set; }


        public async Task<IActionResult> OnGet(string name)
        {
            var user = await UserManager.FindByNameAsync(name);
            if (user == null)
                return NotFound();
            ViewData["User"] = user;
            return Page();
        }
    }
}