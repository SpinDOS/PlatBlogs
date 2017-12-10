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
using PlatBlogs.Views;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    public class SearchController : OffsetCountBaseController
    {
        public SearchController(DbConnection dbConnection): base(dbConnection) { }

        public static int UsersPortion => 1;
        public static int PostsPortion => 1;

        private bool CheckQ(string q) => !string.IsNullOrWhiteSpace(q) && q.Length <= 50;

        public async Task<IActionResult> Index([FromQuery] string q)
        {
            if (!CheckQ(q))
                return BadRequest();

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, User.Identity.Name, User);
            ViewData["User"] = userLeftMenuModel;

            var users = await SearchUsersAsync(userLeftMenuModel.Id, 0, UsersPortion, q);
            users.DefaultText = "No users found";
            var posts = await SearchPostsAsync(userLeftMenuModel.Id, 0, PostsPortion, q);
            posts.DefaultText = "No posts found";

            var model = new SearchModel()
            {
                Q = q,
                Users = users,
                Posts = posts,
            };

            return View("~/Views/Search.cshtml", model);
        }



        public async Task<IActionResult> Users([FromQuery] string q, [FromQuery] int offset = 0)
        {
            if (!CheckQ(q))
                return BadRequest();
            Task<ListWithLoadMoreModel> UsersLoader(string id, int offset_, int count, IAuthor author) // local function
                => SearchUsersAsync(id, offset_, count, q);
            return await base.Get(User.Identity.Name, UsersLoader, offset, PostsPortion,
                u => "No users found for " + q, u => "Search users", u => "Search users: " + q);
        }


        [HttpPost]
        [ActionName("Users")]
        public async Task<IActionResult> UsersPost([FromForm] string q, [FromForm] int offset)
        {
            if (!CheckQ(q))
                return BadRequest();

            Task<ListWithLoadMoreModel> UsersLoader(string id, int offset_, int count, IAuthor author) // local function
                => SearchUsersAsync(id, offset_, count, q);
            return await base.Post(User.Identity.Name, UsersLoader, offset, PostsPortion);
        }

        private async Task<ListWithLoadMoreModel> SearchUsersAsync(string myId, int offset, int count, string q)
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
                    {
                        return offset == 0 ? result : null;
                    }
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







        public async Task<IActionResult> Posts([FromQuery] string q, [FromQuery] int offset = 0)
        {
            if (!CheckQ(q))
                return BadRequest();

            Task<ListWithLoadMoreModel> PostsLoader(string id, int offset_, int count, IAuthor author) // local function
                => SearchPostsAsync(id, offset_, count, q);
            return await base.Get(User.Identity.Name, PostsLoader, offset, PostsPortion,
                u => "No posts found for " + q, u => "Search posts", u => "Search posts: " + q);
        }


        [HttpPost]
        [ActionName("Posts")]
        public async Task<IActionResult> PostsPost([FromForm] string q, [FromForm] int offset)
        {
            if (!CheckQ(q))
                return BadRequest();

            Task<ListWithLoadMoreModel> UsersLoader(string id, int offset_, int count, IAuthor author) // local function
                => SearchPostsAsync(id, offset_, count, q);
            return await base.Post(User.Identity.Name, UsersLoader, offset, PostsPortion);
        }

        private async Task<ListWithLoadMoreModel> SearchPostsAsync(string myId, int offset, int count, string q)
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
                    {
                        return offset == 0 ? result : null;
                    }
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


    }
}