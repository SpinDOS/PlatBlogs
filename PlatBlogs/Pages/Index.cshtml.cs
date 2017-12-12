using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlatBlogs.Data;

namespace PlatBlogs.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(SignInManager<ApplicationUser> signInManager) { _signInManager = signInManager; }

        private SignInManager<ApplicationUser> _signInManager;
        public IActionResult OnGet()
        {
            if (_signInManager.IsSignedIn(User))
                return LocalRedirect("/News");
            return Page();
        }
    }
}
