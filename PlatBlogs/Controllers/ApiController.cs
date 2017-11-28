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
using PlatBlogs.Helpers;
using PlatBlogs.Pages;
using PlatBlogs.Pages._Partials;
using PlatBlogs.Views;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        private DbConnection DbConnection { get; }
        private ApplicationDbContext DbContext { get; }

        public ApiController(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            DbConnection = DbContext.Database.GetDbConnection();
            DbConnection.Open();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbConnection.Dispose();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Like([FromForm] string author, [FromForm] int postId)
        {
            var authorId = await DbConnection.GetUserIdByNameAsync(author);
            if (authorId == null)
            {
                return new JsonResult(new { error = $"User {author} not found" });
            }
            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);
            if (!await DbConnection.IsOpenedAsync(authorId, myId))
            {
                return new JsonResult(new { error = $"Cannot access {author}'s posts: private profile" });
            }

            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM Posts WHERE AuthorId='{authorId}' AND Id='{postId}'";
                if (await command.ExecuteScalarAsync() == null)
                {
                    return new JsonResult(new { error = $"{author}'s post {postId} not found" });
                }
            }

            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
                    "IF NOT EXISTS " +
                        $"(SELECT * FROM Likes WHERE LikerId='{myId}' AND LikedUserId='{authorId}' AND LikedPostId='{postId}') " +
                    $"INSERT INTO Likes (LikerId, LikedUserId, LikedPostId) VALUES ('{myId}', '{authorId}', '{postId}');";
                return await command.ExecuteNonQueryAsync() == 1? 
                    new JsonResult(new { liked = true }) :
                    new JsonResult(new { liked = true, warning = $"{author}'s post {postId} was already liked" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Unlike([FromForm] string author, [FromForm] int postId)
        {
            var authorId = await DbConnection.GetUserIdByNameAsync(author);
            if (authorId == null)
            {
                return new JsonResult(new { error = $"User {author} not found" });
            }
            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM Posts WHERE AuthorId='{authorId}' AND Id='{postId}'";
                if (await command.ExecuteScalarAsync() == null)
                {
                    return new JsonResult(new { error = $"{author}'s post {postId} not found" });
                }
            }

            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
                    $"DELETE FROM Likes WHERE LikerId='{myId}' AND LikedUserId='{authorId}' AND LikedPostId='{postId}'";
                return await command.ExecuteNonQueryAsync() == 1 ?
                    new JsonResult(new { liked = false }) :
                    new JsonResult(new { liked = false, warning = $"{author}'s post {postId} was not liked" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Follow([FromForm] string userName)
        {
            if (User.Identity.Name == userName)
            {
                return new JsonResult(new { error = "You cannot follow yourself" });
            }
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