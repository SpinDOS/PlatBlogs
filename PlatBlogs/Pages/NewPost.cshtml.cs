using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Pages
{
    public class NewPostModel : PageModel
    {
        public NewPostModel(DbConnection dbConnection) { DbConnection = dbConnection; }
        public class InputModel
        {
            [Required]
            [StringLength(450, ErrorMessage = "The post text length must be at max {1} characters long.", MinimumLength = 1)]
            [DataType(DataType.MultilineText)]
            public string Text { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        private DbConnection DbConnection { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                using (var cmd = DbConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@text", Input.Text);
                    cmd.Parameters.AddWithValue("@author", await DbConnection.GetUserIdByNameAsync(User.Identity.Name));

                    cmd.CommandText = "NewPost";
                    if (cmd.ExecuteNonQuery() == 1)
                        return RedirectToPage("/Home");
                    ModelState.AddModelError(string.Empty, "Error writing text to database");
                }
            }
            return Page();
        }
    }
}