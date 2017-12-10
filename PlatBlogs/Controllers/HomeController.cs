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
using PlatBlogs.Interfaces;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    [Authorize]
    [OffsetExceptionFilter]
    public class HomeController : OffsetCountBaseController
    {
        public HomeController(DbConnection dbConnection) : base(dbConnection) { }
        public static int PostsPortion => 1;

        private ItemsLoaderDelegate PostsLoader =>
            ((string id, int offset, int count, IAuthor author) => GetHomePostsAsync(id, offset, count));

        public async Task<IActionResult> Index([FromQuery] int offset = 0)
        {
            return await base.Get(User.Identity.Name, PostsLoader, offset, PostsPortion,
                u => "No news yet", u => "Home", u => "Your news");
        }


        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost([FromForm] int offset)
        {
            return await base.Post(User.Identity.Name, PostsLoader, offset, PostsPortion);
        }

        private async Task<ListWithLoadMoreModel> GetHomePostsAsync(string myId, int offset, int count)
        {
            ListWithLoadMoreModel result = new ListWithLoadMoreModel();
            
            var query = 

$@" 
SELECT {QueryBuildHelpers.SelectFields.PostView("U", "P")} 
FROM Posts P JOIN AspNetUsers U ON P.AuthorId = U.Id 
{QueryBuildHelpers.CrossApply.LikesCounts(myId, "U", "P")} 
WHERE {QueryBuildHelpers.WhereClause.FollowedUsersWhereClause(myId, "U")} 
ORDER BY P.DateTime DESC 
{QueryBuildHelpers.OffsetCount.FetchWithOffsetWithReserveBlock(offset, count)} 
";

            using (var cmd = DbConnection.CreateCommand())
            {
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
        
    }
}