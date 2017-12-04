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
    public class FollowingsController : Controller
    {
        public FollowingsController(DbConnection dbConnection) { DbConnection = dbConnection; }
        public DbConnection DbConnection { get; set; }

        [HttpGet("/followings/{name}")]
        public async Task<IActionResult> Followings(string name, int offset = 0)
        {
            int count = PostsPortion;
            int sum = OffsetCountResolver.ResolveOffsetCount(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, name, User);
            if (userLeftMenuModel == null)
                return NotFound();

            var followings = await GetUsersAsync(name, 0, sum, true);
            if (followings.DefaultText == null)
                followings.DefaultText = "No followings yet";

            ViewData["User"] = userLeftMenuModel;
            ViewData["Title"] = $"{userLeftMenuModel.UserName}'s followings";
            ViewData["Main"] = $"{userLeftMenuModel.FullName}'s followings";

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", followings);
        }


        [HttpPost("/followings/{name}")]
        public async Task<IActionResult> FollowingsPost(string name, [FromForm] int offset)
        {
            int count = PostsPortion;
            OffsetCountResolver.ResolveOffsetCount(offset, ref count);

            var followings = await GetUsersAsync(name, offset, count, true);
            if (followings == null)
                return NotFound();
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", followings);
        }


        [HttpGet("/followers/{name}")]
        public async Task<IActionResult> Followers(string name, int offset = 0)
        {
            int count = PostsPortion;
            int sum = OffsetCountResolver.ResolveOffsetCount(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, name, User);
            if (userLeftMenuModel == null)
                return NotFound();

            var followers = await GetUsersAsync(name, 0, sum, false);
            if (followers.DefaultText == null)
                followers.DefaultText = "No followers yet";

            ViewData["User"] = userLeftMenuModel;
            ViewData["Title"] = $"{userLeftMenuModel.UserName}'s followers";
            ViewData["Main"] = $"{userLeftMenuModel.FullName}'s followers";

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", followers);
        }


        [HttpPost("/followers/{name}")]
        public async Task<IActionResult> FollowersPost(string name, [FromForm] int offset)
        {
            int count = PostsPortion;
            OffsetCountResolver.ResolveOffsetCount(offset, ref count);

            var followers = await GetUsersAsync(name, offset, count, false);
            if (followers == null)
                return NotFound();
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", followers);
        }



        private async Task<ListWithLoadMoreModel> GetUsersAsync(string name, int offset, int count, bool followings)
        {
            var userId = await DbConnection.GetUserIdByNameAsync(name);
            if (userId == null)
                return null;

            ListWithLoadMoreModel result = new ListWithLoadMoreModel();
            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.CommandText =
$@"SELECT Id, FullName, UserName, AvatarPath, PublicProfile, ShortInfo 
FROM AspNetUsers WHERE Id IN 
(SELECT Followe{(followings ? "d" : "r")}Id FROM Followers 
  WHERE Followe{(followings ? "r" : "d")}Id = '{userId}') 
ORDER BY Id 
OFFSET {offset} ROWS 
FETCH NEXT {count} ROWS ONLY
";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return result;
                    result.Elements = await UserViewModel.FromSqlReaderAsync(reader);
                    if (result.Elements.Count == count)
                    {
                        result.LoadMoreModel = new LoadMoreModel($"/Follow{(followings? "ing": "er")}s/{name}")
                        {
                            Offset = offset + count,
                        };
                    }
                }
            }
            return result;
        }

        public static int PostsPortion => 1;
    }
}