using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PlatBlogs.Data;
using PlatBlogs.Extensions;
using PlatBlogs.Helpers;
using PlatBlogs.Pages._Partials;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Pages
{
    public class UserModel : PageModel
    {
        public UserModel(ApplicationDbContext dbContext) { DbContext = dbContext; }

        public ApplicationDbContext DbContext { get; set; }

        public PostListWithLoadMoreModel PostsModel { get; set; } = new PostListWithLoadMoreModel();
        public ApplicationUser InspectedUser { get; set; }
        public bool AjaxResult { get; set; }
        public bool ClosedProfile { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(string name, [FromQuery] int offset = 0, [FromQuery] bool ajax = false)
        {
            var user = await DbContext.Users
                .FromSql($"SELECT * FROM AspNetUsers WHERE NormalizedUserName={name.ToUpper()}")
                .FirstOrDefaultAsync();
            if (user == null)
                return NotFound();

            InspectedUser = user;

            AjaxResult = ajax;
            if (!ajax)
                PostsModel.DefaultText = user.FullName + " doesn't have post yet";

            var count = PostsPortion;
            var overflow = OffsetCountResolver.ResolveOffsetCount(ref offset, ref count, ajax);

            using (var conn = DbContext.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                var myId = await conn.GetUserIdByNameAsync(User.Identity.Name);

                if (!user.PublicProfile && !await conn.IsOpenedForViewerAsync(user.Id, myId))
                {
                    ClosedProfile = true;
                    return Page();
                }

                PostsModel.Posts = await conn.SimpleQueryPosts(myId, where: names => $"WHERE {names.AuthorId}='{user.Id}'", 
                    offset: offset, count: count);

                PostsModel.MorePostsExist = PostsModel.Posts.Count == count && !overflow;
                if (PostsModel.MorePostsExist)
                {
                    PostsModel.LoadMoreModel = new LoadMoreModel()
                    {
                        Action = "/User/" + name,
                        Offset = offset + count,
                    };
                }
            }
            return Page();
        }

        public static int PostsPortion => 1;
    }
}