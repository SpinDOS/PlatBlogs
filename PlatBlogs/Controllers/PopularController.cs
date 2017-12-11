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
    public class PopularController : OffsetCountBaseController
    {
        public PopularController(DbConnection dbConnection) : base(dbConnection) { }

        public static int PostsPortion => 2;

        private ItemsLoaderDelegate PostsLoader =>
            ((string id, int offset, int count, IAuthor author) => GetPopularPostsAsync(id, offset, count));

        public async Task<IActionResult> Index([FromQuery] int offset = 0)
        {
            return await base.Get(User.Identity.Name, PostsLoader, offset, PostsPortion,
                u => "No popular news yet", u => "Popular", u => "Popular news");
        }


        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost([FromForm] int offset)
        {
            return await base.Post(User.Identity.Name, PostsLoader, offset, PostsPortion);
        }

        private async Task<ListWithLoadMoreModel> GetPopularPostsAsync(string myId, int offset, int count, IUser me = null)
        {
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
                    return await reader.ReadToListWithLoadMoreModel(offset, count, PostViewModel.FromSqlReaderAsync,
                        () => new LoadMoreModel("/Popular"));
                }
            }
        }


    }
}