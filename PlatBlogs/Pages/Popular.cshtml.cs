using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PlatBlogs.Data;
using PlatBlogs.Extensions;
using PlatBlogs.Helpers;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Pages
{
    public class PopularModel : PageModel
    {
        public PopularModel(ApplicationDbContext dbContext) { DbContext = dbContext; }

        public ApplicationDbContext DbContext { get; set; }

        public PostListWithLoadMoreModel PostsModel { get; set; } = new PostListWithLoadMoreModel();
        public ApplicationUser CurrentUser { get; set; }
        public bool AjaxResult { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] int offset = 0, [FromQuery] bool ajax = false)
        {
            CurrentUser = await DbContext.ApplicationUser
                .FromSql("SELECT * FROM AspNetUsers " +
                         $"WHERE NormalizedUserName = '{User.Identity.Name.ToUpper()}'")
                .FirstAsync();

            AjaxResult = ajax;
            PostsModel.DefaultText = "No popular news yet";
            var count = PostsPortion;
            var overflow = OffsetCountResolver.ResolveOffsetCount(ref offset, ref count, ajax);

            using (var conn = DbContext.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                PostsModel.Posts = await conn.SimpleQueryPosts(CurrentUser.Id,
                    orderBy: names => 
$@"ORDER BY {names.LikesCount} - DATEDIFF(DAY, {names.PostTime}, GETDATE()) DESC, 
DATEDIFF(SECOND, {names.PostTime}, GETDATE()) ASC ",
                    offset: offset, count: count);

                PostsModel.MorePostsExist = PostsModel.Posts.Count == count && !overflow;
                if (PostsModel.MorePostsExist)
                {
                    PostsModel.LoadMoreModel = new LoadMoreModel()
                    {
                        Action = "/Popular",
                        Offset = offset + count,
                    };
                }
            }
            return Page();
        }

        public static int PostsPortion => 1;
    }
}