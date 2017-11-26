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
using PlatBlogs.Extensions;

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

        public ApiController(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
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
        public async Task<JsonResult> Follow([FromForm] string userName)
        {
            int affectedRows = 0;
            using (var conn = DbContext.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                var userId = DbConnectionExtensions.GetUserIdByName(conn, userName);
                if (userId == null)
                {
                    return new JsonResult(new { error = $"User {userName} not found" });
                }
                var myId = DbConnectionExtensions.GetUserIdByName(conn, User.Identity.Name);

                using (var command = conn.CreateCommand())
                {
                    command.CommandText =
                        $"IF NOT EXISTS (SELECT * FROM Followers WHERE FollowedId='{userId}' AND FollowerId='{myId}')" +
                        $"INSERT INTO Followers (FollowedId, FollowerId) VALUES ('{userId}', '{myId}');";
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            return affectedRows == 1? 
                new JsonResult(new { followed = true }) :
                new JsonResult(new { followed = true, warning = $"User {userName} was already followed" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Unfollow([FromForm] string userName)
        {
            int affectedRows = 0;
            using (var conn = DbContext.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                var userId = DbConnectionExtensions.GetUserIdByName(conn, userName);
                if (userId == null)
                {
                    return new JsonResult(new { error = $"User {userName} not found" });
                }
                var myId = DbConnectionExtensions.GetUserIdByName(conn, User.Identity.Name);

                using (var command = conn.CreateCommand())
                {
                    command.CommandText =
                        $"DELETE FROM Followers WHERE FollowedId='{userId}' AND FollowerId='{myId}'";
                    affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
            return affectedRows == 1 ?
                new JsonResult(new { followed = false }) :
                new JsonResult(new { followed = false, warning = $"User {userName} was not followed by you" });
        }
    }
}