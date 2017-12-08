﻿using System;
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
    public class PopularController : Controller
    {
        public PopularController(DbConnection dbConnection) { DbConnection = dbConnection; }
        public DbConnection DbConnection { get; set; }


        public async Task<IActionResult> Index([FromQuery] int offset = 0)
        {
            int count = PostsPortion;
            int sum = OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, User.Identity.Name, User);

            var posts = await GetPopularPostsAsync(userLeftMenuModel.Id, 0, sum);
            posts.DefaultText = "No popular news yet";

            ViewData["User"] = userLeftMenuModel;
            ViewData["Title"] = "Popular";
            ViewData["Main"] = "Popular news";

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", posts);
        }


        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost([FromForm] int offset)
        {
            int count = PostsPortion;
            OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            var posts = await GetPopularPostsAsync(myId, offset, count);
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", posts);
        }

        private async Task<ListWithLoadMoreModel> GetPopularPostsAsync(string myId, int offset, int count)
        {
            ListWithLoadMoreModel result = new ListWithLoadMoreModel();
            
            var query =
$@" 
SELECT {QueryBuildHelpers.SelectFields.PostView("U", "P")} 
FROM Posts P JOIN AspNetUsers U ON P.AuthorId = U.Id 
{QueryBuildHelpers.CrossApply.LikesCounts(myId, "U", "P")} 
WHERE {QueryBuildHelpers.WhereClause.OpenedUsersWhereClause(myId, "U")} 
ORDER BY (AllLikesCount - DATEDIFF(DAY, P.DateTime, GETDATE()) ) DESC, P.DateTime DESC 
{QueryBuildHelpers.OffsetCount.FetchWithOffsetWithReserveBlock(offset, count)} 
";

            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return result;
                    var posts = await PostViewModel.FromSqlReaderAsync(reader);
                    if (posts.Count == count + 1)
                    {
                        posts.RemoveAt(posts.Count - 1);
                        result.LoadMoreModel = new LoadMoreModel("/Popular")
                        {
                            Offset = offset + count,
                        };
                    }
                    result.Elements = posts;
                }
            }
            return result;
        }


        public static int PostsPortion => 2;
    }
}