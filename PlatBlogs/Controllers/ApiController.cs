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
using PlatBlogs.Views;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class ApiController : Controller
    {
        public ApiController(DbConnection dbConnection) { DbConnection = dbConnection; }
        private DbConnection DbConnection { get; }

        [HttpPost]
        public async Task<JsonResult> Like([FromForm] string author, [FromForm] int postId)
        {
            (string authorId, bool authorPublicProfile) = await GetIdAndPublicProfile(author);
            if (authorId == null)
                return new JsonResult(new { error = $"User {author} not found" });

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);
            if (!authorPublicProfile)
            {
                if (!await DbConnection.CheckFollowingAsync(myId, authorId))
                    return new JsonResult(new { error = $"Cannot access {author}'s posts: private profile" });
            }

            if (!await CheckPostExists(authorId, postId))
                return new JsonResult(new { error = $"{author}'s post {postId} not found" });
            
            return await CreateLikeIfNotExists(authorId, postId, myId)?
                new JsonResult(new { liked = true }) :
                new JsonResult(new { liked = true, warning = $"{author}'s post {postId} was already liked" });;
        }


        [HttpPost]
        public async Task<JsonResult> Unlike([FromForm] string author, [FromForm] int postId)
        {
            var authorId = await DbConnection.GetUserIdByNameAsync(author);
            if (authorId == null)
                return new JsonResult(new { error = $"User {author} not found" });

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            if (!await CheckPostExists(authorId, postId))
                return new JsonResult(new { error = $"{author}'s post {postId} not found" });

            return await DeleteLikeIfExists(authorId, postId, myId)?
                new JsonResult(new { liked = false }) :
                new JsonResult(new { liked = false, warning = $"{author}'s post {postId} was not liked" });
        }

        [HttpPost]
        public async Task<JsonResult> Follow([FromForm] string userName)
        {
            if (userName.ToUpper() == User.Identity.Name.ToUpper())
                return new JsonResult(new { error = "You cannot follow yourself" });

            var userId = await DbConnection.GetUserIdByNameAsync(userName);
            if (userId == null)
                return new JsonResult(new { error = $"User {userName} not found" });

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            return await FollowIfNotFollowed(userId, myId)?
                new JsonResult(new { followed = true }) :
                new JsonResult(new { followed = true, warning = $"User {userName} was already followed" });
        }

        [HttpPost]
        public async Task<JsonResult> Unfollow([FromForm] string userName)
        {
            var userId = await DbConnection.GetUserIdByNameAsync(userName);
            if (userId == null)
                return new JsonResult(new { error = $"User {userName} not found" });

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            return await UnFollowIfFollowed(userId, myId)?
                new JsonResult(new { followed = false }) :
                new JsonResult(new { followed = false, warning = $"User {userName} was not followed by you" });
        }
        

        private async Task<(string, bool)> GetIdAndPublicProfile(string authorName)
        {
            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@normalizedUserName", authorName.ToUpper());
                cmd.CommandText = "SELECT Id, PublicProfile FROM AspNetUsers WHERE NormalizedUserName = @normalizedUserName";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return await reader.ReadAsync() ?
                        (reader.GetString(0), reader.GetBoolean(1)) :
                        (null, false);
                }
            }
        }

        private async Task<bool> CheckPostExists(string authorId, int postId)
        {
            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT 1 FROM Posts WHERE AuthorId='{authorId}' AND Id='{postId}'; ";
                return await command.ExecuteScalarAsync() != null;
            }
        }

        private async Task<bool> CreateLikeIfNotExists(string authorId, int postId, string myId)
        {
            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
$@"IF NOT EXISTS 
    (SELECT * FROM Likes WHERE LikedUserId='{authorId}' AND LikedPostId='{postId}' AND LikerId='{myId}') 
    INSERT INTO Likes (LikedUserId, LikedPostId, LikerId) VALUES ('{authorId}', '{postId}', '{myId}'); ";
                return await command.ExecuteNonQueryAsync() == 1;
            }
        }
        

        private async Task<bool> DeleteLikeIfExists(string authorId, int postId, string myId)
        {
            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
                    $"DELETE FROM Likes WHERE LikedUserId='{authorId}' AND LikedPostId='{postId}' AND LikerId='{myId}'; ";
                return await command.ExecuteNonQueryAsync() == 1;
            }
        }


        private async Task<bool> FollowIfNotFollowed(string userId, string myId)
        {
            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
$@"IF NOT EXISTS (SELECT * FROM Followers WHERE FollowedId='{userId}' AND FollowerId='{myId}') 
    INSERT INTO Followers (FollowedId, FollowerId) VALUES ('{userId}', '{myId}'); ";
                return await command.ExecuteNonQueryAsync() == 1;
            }
        }
        

        private async Task<bool> UnFollowIfFollowed(string userId, string myId)
        {
            using (var command = DbConnection.CreateCommand())
            {
                command.CommandText =
                    $"DELETE FROM Followers WHERE FollowedId='{userId}' AND FollowerId='{myId}'; ";
                return await command.ExecuteNonQueryAsync() == 1;
            }
        }

    }
}