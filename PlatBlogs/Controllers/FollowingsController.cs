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
using PlatBlogs.Interfaces;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    [Authorize]
    [OffsetExceptionFilter]
    public class FollowingsController : OffsetCountBaseController
    {
        public FollowingsController(DbConnection dbConnection) : base(dbConnection) { }
        public static int PostsPortion => 1;

        [HttpGet("/followings/{name}")]
        public async Task<IActionResult> Followings(string name, int offset = 0)
        {
            Task<ListWithLoadMoreModel> UsersLoader(string id, int offset_, int count, IAuthor author) // local function
                => GetUsersAsync(id, offset_, count, name, true);
            return await base.Get(name, UsersLoader, offset, PostsPortion,
                u => "No followings yet", u => $"{u.UserName}'s followings", u => $"{u.FullName}'s followings");
        }


        [HttpPost("/followings/{name}")]
        public async Task<IActionResult> FollowingsPost(string name, [FromForm] int offset)
        {
            Task<ListWithLoadMoreModel> UsersLoader(string id, int offset_, int count, IAuthor author) // local function
                => GetUsersAsync(id, offset_, count, name, true);
            return await base.Post(name, UsersLoader, offset, PostsPortion);
        }


        [HttpGet("/followers/{name}")]
        public async Task<IActionResult> Followers(string name, int offset = 0)
        {
            Task<ListWithLoadMoreModel> UsersLoader(string id, int offset_, int count, IAuthor author) // local function
                => GetUsersAsync(id, offset_, count, name, false);
            return await base.Get(name, UsersLoader, offset, PostsPortion,
                u => "No followers yet", u => $"{u.UserName}'s followers", u => $"{u.FullName}'s followers");
        }


        [HttpPost("/followers/{name}")]
        public async Task<IActionResult> FollowersPost(string name, [FromForm] int offset)
        {
            Task<ListWithLoadMoreModel> UsersLoader(string id, int offset_, int count, IAuthor author) // local function
                => GetUsersAsync(id, offset_, count, name, false);
            return await base.Post(name, UsersLoader, offset, PostsPortion);
        }



        private async Task<ListWithLoadMoreModel> GetUsersAsync(string userId, int offset, int count, 
            string name, bool followings)
        {
            var query = 
$@" 
SELECT {QueryBuildHelpers.SelectFields.UserView("U")} 
FROM AspNetUsers U 
WHERE U.Id IN (SELECT Followe{(followings ? "d" : "r")}Id FROM Followers 
               WHERE Followe{(followings ? "r" : "d")}Id = '{userId}') 
ORDER BY Id 
{QueryBuildHelpers.OffsetCount.FetchWithOffsetWithReserveBlock(offset, count)} 
";

            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return await reader.ReadToListWithLoadMoreModel(offset, count, UserViewModel.FromSqlReaderAsync,
                        () => new LoadMoreModel($"/Follow{(followings ? "ing" : "er")}s/{name}"));
                }
            }
        }

    }
}