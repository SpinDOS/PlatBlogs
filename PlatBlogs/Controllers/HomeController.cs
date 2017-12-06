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
using static PlatBlogs.Helpers.QueryBuildHelpers.Posts;

namespace PlatBlogs.Controllers
{
    [Authorize]
    [OffsetExceptionFilter]
    public class HomeController : Controller
    {
        public HomeController(DbConnection dbConnection) { DbConnection = dbConnection; }
        public DbConnection DbConnection { get; set; }


        public async Task<IActionResult> Index([FromQuery] int offset = 0)
        {
            int count = PostsPortion;
            int sum = OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, User.Identity.Name, User);

            var posts = await GetHomePostsAsync(userLeftMenuModel.Id, 0, sum);
            posts.DefaultText = "No news yet";

            ViewData["User"] = userLeftMenuModel;
            ViewData["Title"] = "Home";
            ViewData["Main"] = "Your news";

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", posts);
        }


        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost([FromForm] int offset)
        {
            int count = PostsPortion;
            OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            var posts = await GetHomePostsAsync(myId, offset, count);
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", posts);
        }

        private async Task<ListWithLoadMoreModel> GetHomePostsAsync(string myId, int offset, int count)
        {
            ListWithLoadMoreModel result = new ListWithLoadMoreModel();

            var whereClause = QueryBuildHelpers.WhereClause.FollowedUsersWhereClause(myId);
            var authorsQuery = QueryBuildHelpers.Users.AuthorsQuery(whereClause);
            var postsWithAuthorsQuery = QueryBuildHelpers.Posts.PostsWithAuthorsQuery(authorsQuery);
            var query = 

$@"{QueryBuildHelpers.Posts.PostViewsQuery(myId, postsWithAuthorsQuery)} 
ORDER BY {nameof(FieldNames.PostDateTime)} DESC
{QueryBuildHelpers.OffsetCount.FetchWithOffsetWithReserveBlock(offset, count)} ";

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
                        result.LoadMoreModel = new LoadMoreModel("/Home")
                        {
                            Offset = offset + count,
                        };
                    }
                    result.Elements = posts;
                }
            }
            return result;
        }


        public static int PostsPortion => 1;
    }
}