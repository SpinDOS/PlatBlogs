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
using PlatBlogs.Views;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    [Authorize]
    [OffsetExceptionFilter]
    public class SearchController : Controller
    {
        public SearchController(DbConnection dbConnection) { DbConnection = dbConnection; }
        public DbConnection DbConnection { get; set; }

        public async Task<IActionResult> Index([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length > 50)
                return BadRequest();

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, User.Identity.Name, User);
            ViewData["User"] = userLeftMenuModel;

            var users = await SearchUsersAsync(userLeftMenuModel.Id, q, 0, UsersPortion);
            users.DefaultText = "No users found";
            var posts = await SearchPostsAsync(userLeftMenuModel.Id, q, 0, PostsPortion);
            posts.DefaultText = "No posts found";

            var model = new SearchModel()
            {
                Q = q,
                Users = users,
                Posts = posts,
            };

            return View("~/Views/Search.cshtml", model);
        }

        public async Task<IActionResult> Posts([FromQuery] string q, [FromQuery] int offset = 0)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length > 50)
                return BadRequest();

            int count = PostsPortion;
            int sum = OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, User.Identity.Name, User);

            var posts = await SearchPostsAsync(userLeftMenuModel.Id, q, 0, sum);
            posts.DefaultText = "No posts found for " + q;

            ViewData["User"] = userLeftMenuModel;
            ViewData["Main"] = "Search posts: " + q;

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", posts);
        }


        [HttpPost]
        [ActionName("Posts")]
        public async Task<IActionResult> PostsPost([FromForm] string q, [FromForm] int offset)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length > 50)
                return BadRequest();

            int count = PostsPortion;
            OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            var posts = await SearchPostsAsync(myId, q, offset, count);
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", posts);
        }

        private async Task<ListWithLoadMoreModel> SearchPostsAsync(string myId, string q, int offset, int count)
        {
            ListWithLoadMoreModel result = new ListWithLoadMoreModel();

            var query =
$@" 
SELECT {QueryBuildHelpers.SelectFields.PostView("U", "P")} 
FROM Posts P JOIN AspNetUsers U ON P.AuthorId = U.Id 
{QueryBuildHelpers.CrossApply.LikesCounts(myId, "U", "P")} 
CROSS APPLY (SELECT Pos = CHARINDEX(@q, UPPER(P.Message)) ) _PosCalculation
WHERE ({QueryBuildHelpers.WhereClause.OpenedUsersWhereClause(myId, "U")}) AND Pos > 0
ORDER BY Pos ASC, LEN(P.Message) ASC 
{QueryBuildHelpers.OffsetCount.FetchWithOffsetWithReserveBlock(offset, count)} 
";

            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@q", q.ToUpper());
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return result;
                    var posts = await PostViewModel.FromSqlReaderAsync(reader);
                    if (posts.Count == count + 1)
                    {
                        posts.RemoveAt(posts.Count - 1);
                        result.LoadMoreModel = new LoadMoreModel("/search/posts")
                        {
                            Offset = offset + count,
                        };
                        result.LoadMoreModel.AdditionalFields["q"] = q;
                    }
                    result.Elements = posts;
                }
            }
            return result;
        }

        public async Task<IActionResult> Users([FromQuery] string q, [FromQuery] int offset = 0)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length > 50)
                return BadRequest();

            int count = UsersPortion;
            int sum = OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, User.Identity.Name, User);

            var users = await SearchUsersAsync(userLeftMenuModel.Id, q, 0, sum);
            users.DefaultText = "No users found for " + q;

            ViewData["User"] = userLeftMenuModel;
            ViewData["Title"] = "Search";
            ViewData["Main"] = "Search users: " + q;

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", users);
        }


        [HttpPost]
        [ActionName("Users")]
        public async Task<IActionResult> UsersPost([FromForm] string q, [FromForm] int offset)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length > 50)
                return BadRequest();

            int count = UsersPortion;
            OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            var users = await SearchUsersAsync(myId, q, offset, count);
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", users);
        }

        private async Task<ListWithLoadMoreModel> SearchUsersAsync(string myId, string q, int offset, int count)
        {
            ListWithLoadMoreModel result = new ListWithLoadMoreModel();

            var quertyPosLen = 
$@"
SELECT {QueryBuildHelpers.SelectFields.UserView("I")}, 
    CASE 
        WHEN FullNamePos > 0 AND UserNamePos > 3 THEN 
            CASE 
                WHEN FullNamePos > UserNamePos THEN UserNamePos
                ELSE FullNamePos
            END
        WHEN FullNamePos > 0 THEN FullNamePos
        ELSE UserNamePos
    END AS Pos

FROM AspNetUsers I 
CROSS APPLY (SELECT FullNamePos = CHARINDEX(@q, UPPER(I.FullName)) ) _FullNamePosCalculation 
CROSS APPLY (SELECT UserNamePos = CHARINDEX(@q, UPPER(I.UserName)) + 3 ) _UserNamePosCalculation 
WHERE FullNamePos > 0 OR UserNamePos > 3 
";

            var query =
$@" 
SELECT {QueryBuildHelpers.SelectFields.UserView("U")} 
FROM ({quertyPosLen}) U 
ORDER BY U.Pos ASC 
{QueryBuildHelpers.OffsetCount.FetchWithOffsetWithReserveBlock(offset, count)} 
";

            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@q", q.ToUpper());
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return result;
                    var posts = await UserViewModel.FromSqlReaderAsync(reader);
                    if (posts.Count == count + 1)
                    {
                        posts.RemoveAt(posts.Count - 1);
                        result.LoadMoreModel = new LoadMoreModel("/search/users")
                        {
                            Offset = offset + count,
                        };
                        result.LoadMoreModel.AdditionalFields["q"] = q;
                    }
                    result.Elements = posts;
                }
            }
            return result;
        }


        public static int UsersPortion => 1;
        public static int PostsPortion => 1;
    }
}