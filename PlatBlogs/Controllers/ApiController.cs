using System;
using System.Collections.Generic;
using System.Data;
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
        private DbConnection DbConnection { get; set; }
        public ApiController(DbConnection dbConnection) { DbConnection = dbConnection; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Follow([FromForm] string userName)
        {
            var userId = await DbConnection.GetUserIdByNameAsync(userName);
            if (userId == null)
            {
                return new JsonResult(new { error = $"User {userName} not found" });
            }
            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
                    $"IF NOT EXISTS (SELECT * FROM Followers WHERE FollowedId='{userId}' AND FollowerId='{myId}')" +
                    $"INSERT INTO Followers (FollowedId, FollowerId) VALUES ('{userId}', '{myId}');";
                return await command.ExecuteNonQueryAsync() == 1 ?
                    new JsonResult(new { followed = true }) :
                    new JsonResult(new { followed = true, warning = $"User {userName} was already followed" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Unfollow([FromForm] string userName)
        {
            var userId = await DbConnection.GetUserIdByNameAsync(userName);
            if (userId == null)
            {
                return new JsonResult(new { error = $"User {userName} not found" });
            }
            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
                    $"DELETE FROM Followers WHERE FollowedId='{userId}' AND FollowerId='{myId}'";
                return await command.ExecuteNonQueryAsync() == 1 ?
                    new JsonResult(new { followed = false }) :
                    new JsonResult(new { followed = false, warning = $"User {userName} was not followed by you" });
            }
        }
    }
}