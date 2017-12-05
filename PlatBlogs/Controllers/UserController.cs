using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatBlogs.Attributes;
using PlatBlogs.Extensions;
using PlatBlogs.Helpers;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    [Authorize]
    [OffsetExceptionFilter]
    public class UserController : Controller
    {
        public UserController(DbConnection dbConnection) { DbConnection = dbConnection; }
        public DbConnection DbConnection { get; set; }

        [HttpGet("/user/{name}")]
        public async Task<IActionResult> Index(string name, [FromQuery] int offset = 0)
        {
            int count = PostsPortion;
            int sum = OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, name, User);
            if (userLeftMenuModel == null)
                return NotFound();

            var posts = await GetPostsAsync(name, 0, sum);
            if (posts.DefaultText == null)
                posts.DefaultText = "No posts yet";

            ViewData["User"] = userLeftMenuModel;
            ViewData["Title"] = $"{userLeftMenuModel.UserName}'s posts";
            ViewData["Main"] = $"{userLeftMenuModel.FullName}'s posts";

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", posts);
        }


        [HttpPost("/user/{name}")]
        public async Task<IActionResult> IndexPost(string name, [FromForm] int offset)
        {
            int count = PostsPortion;
            OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var posts = await GetPostsAsync(name, offset, count);
            if (posts == null)
                return NotFound();
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", posts);
        }

        private async Task<ListWithLoadMoreModel> GetPostsAsync(string name, int offset, int count)
        {
            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            var authorInfo = await GetUserInfo(name, myId);
            if (authorInfo == null)
                return null;

            ListWithLoadMoreModel result = new ListWithLoadMoreModel();
            if (!authorInfo.OpenedToRead)
            {
                result.DefaultText = $"User {authorInfo.UserName} has private profile. " +
                                     $"Only people followed by {authorInfo.UserName} can access posts";
                return result;
            }

            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.CommandText = 
$@"DECLARE @publicProfile BIT;
SET @publicProfile = CAST({Convert.ToInt32(authorInfo.PublicProfile)} AS BIT);

SELECT P.AuthorId, P.Id, P.DateTime, P.Message, 
(SELECT COUNT(*) FROM Likes AllLikes 
    WHERE AllLikes.LikedUserId = '{authorInfo.Id}' AND P.Id = AllLikes.LikedPostId), 
(SELECT COUNT(*) FROM Likes MyLikes 
    WHERE MyLikes.LikedUserId = '{authorInfo.Id}' AND P.Id = MyLikes.LikedPostId AND LikerId = '{myId}'), 
'{authorInfo.FullName}', '{authorInfo.UserName}', @publicProfile 
FROM Posts P
WHERE P.AuthorId = '{authorInfo.Id}' 
ORDER BY P.DateTime DESC 
OFFSET {offset} ROWS 
FETCH NEXT {count + 1} ROWS ONLY";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return result;
                    var posts = await PostViewModel.FromSqlReaderAsync(reader);
                    if (posts.Count == count + 1)
                    {
                        posts.RemoveAt(posts.Count - 1);
                        result.LoadMoreModel = new LoadMoreModel("/user/" + name)
                        {
                            Offset = offset + count,
                        };
                    }
                    result.Elements = posts;
                }
            }
            return result;
        }

        private async Task<UserInfoForPost> GetUserInfo(string name, string viewerId)
        {
            UserInfoForPost result = null;
            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@normalizedUserName", name.ToUpper());
                cmd.CommandText =
@"SELECT Id, FullName, UserName, PublicProfile 
FROM AspNetUsers WHERE NormalizedUserName = @normalizedUserName;";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return null;
                    result = new UserInfoForPost()
                    {
                        Id = reader.GetString(0),
                        FullName = reader.GetString(1),
                        UserName = reader.GetString(2),
                        PublicProfile = reader.GetBoolean(3),
                    };
                }
            }
            result.OpenedToRead = result.PublicProfile || result.Id == viewerId || 
                await DbConnection.CheckFollowingAsync(viewerId, result.Id);
            
            return result;
        }
        public static int PostsPortion => 1;

        private class UserInfoForPost
        {
            public string Id, FullName, UserName;
            public bool PublicProfile, OpenedToRead;
        }
    }
}