using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PlatBlogs.Data;

namespace PlatBlogs.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        public class PostIdentifier
        {
            public int Id { get; set; }
            public string AuthorId { get; set; }
        }

        private ApplicationDbContext DbContext { get; }
        private UserManager<ApplicationUser> UserManager { get; }

        public ApiController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            DbContext = dbContext;
            UserManager = userManager;
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Like([FromForm] PostIdentifier post, [FromForm] string returnUrl)
        //{
        //    return LocalRedirect(Url.GetLocalUrl(returnUrl));
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UnLike([FromForm] PostIdentifier post, [FromForm] string returnUrl)
        //{
        //    return LocalRedirect(Url.GetLocalUrl(returnUrl));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Follow([FromForm] string userId)
        {
            var myId = (await UserManager.GetUserAsync(User)).Id;
            var conn = DbContext.Database.GetDbConnection();
            int affectedRows = 0;
            try
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    command.Parameters.Add(new SqlParameter("FollowerId", myId));
                    command.Parameters.Add(new SqlParameter("FollowedId", userId));
                    command.CommandText =
                        "IF NOT EXISTS (SELECT * FROM Followers WHERE FollowerId = @FollowerId AND FollowedId = @FollowedId)" +
                        "INSERT INTO Followers (FollowerId, FollowedId) VALUES (@FollowerId, @FollowedId)";
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                conn.Close();
            }
            return new JsonResult(new { success = affectedRows == 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Unfollow([FromForm] string userId)
        {
            var myId = (await UserManager.GetUserAsync(User)).Id;
            var conn = DbContext.Database.GetDbConnection();
            int affectedRows = 0;
            try
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    command.Parameters.Add(new SqlParameter("FollowerId", myId));
                    command.Parameters.Add(new SqlParameter("FollowedId", userId));
                    command.CommandText =
                        "DELETE FROM Followers WHERE FollowerId = @FollowerId AND FollowedId = @FollowedId";
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                conn.Close();
            }
            return new JsonResult(new { success = affectedRows == 1 });
        }
    }
}