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
    public class UserController : OffsetCountBaseController
    {
        public UserController(DbConnection dbConnection) : base(dbConnection) { }
        public static int PostsPortion => 1;

        [HttpGet("/user/{name}")]
        public async Task<IActionResult> Index(string name, [FromQuery] int offset = 0)
        {
            ItemsLoaderDelegate postsLoader = GetUserPostsAsync;
            return await base.Get(name, postsLoader, offset, PostsPortion,
                u => "No posts yet", u => $"{u.UserName}'s posts", u => $"{u.FullName}'s posts");
        }


        [HttpPost("/user/{name}")]
        public async Task<IActionResult> IndexPost(string name, [FromForm] int offset)
        {
            ItemsLoaderDelegate postsLoader = GetUserPostsAsync;
            return await base.Post(User.Identity.Name, postsLoader, offset, PostsPortion);
        }

        private async Task<ListWithLoadMoreModel> GetUserPostsAsync(string authorId, int offset, int count, IAuthor author)
        {
            var myId = await DbConnection.GetUserIdByNameAsync(User.Identity.Name);

            if (author == null)
            {
                author = await GetAuthorById(authorId);
            }

            if (!await DbConnection.IsOpenedForViewerAsync(author, myId))
            {
                return new ListWithLoadMoreModel()
                {
                    DefaultText =
                        $"User {author.UserName} has private profile. " +
                        $"Only people followed by {author.UserName} can access posts",
                };
            }

            var singleAuthorQuery =
$@" 
SELECT '{author.Id}'       AS Id, 
       '{author.FullName}' AS FullName, 
       '{author.UserName}' AS UserName, 
        @publicProfile         AS PublicProfile 
";

            var query =
$@"
DECLARE @publicProfile BIT;
SET     @publicProfile = CAST({Convert.ToInt32(author.PublicProfile)} AS BIT);

SELECT {QueryBuildHelpers.SelectFields.PostView("U", "P")} 
FROM ({singleAuthorQuery}) U JOIN Posts P ON U.Id = P.AuthorId 
{QueryBuildHelpers.CrossApply.LikesCounts(myId, "U", "P")} 
ORDER BY P.DateTime DESC 
{QueryBuildHelpers.OffsetCount.FetchWithOffsetWithReserveBlock(offset, count)} 
";

            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return await reader.ReadToListWithLoadMoreModel(offset, count, PostViewModel.FromSqlReaderAsync,
                        () => new LoadMoreModel("/User/" + author.UserName));
                }
            }
        }


        private async Task<IAuthor> GetAuthorById(string userId)
        {
            var query =
$@"
SELECT U.Id, {QueryBuildHelpers.SelectFields.Author("U")} 
FROM AspNetUsers U 
WHERE Id = '{userId}' 
";

            using (var cmd = DbConnection.CreateCommand())
            {
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return null;
                    return new UserViewModel()
                    {
                        Id = reader.GetString(0),
                        FullName = reader.GetString(1),
                        UserName = reader.GetString(2),
                        PublicProfile = reader.GetBoolean(3),
                    };
                }
            }
        }

    }
}