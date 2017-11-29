using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PlatBlogs.Data;
using PlatBlogs.Helpers;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Pages
{
    public class FollowingsModel : PageModel
    {
        public FollowingsModel(ApplicationDbContext dbContext) { DbContext = dbContext; }
        public ApplicationDbContext DbContext { get; }

        public ApplicationUser InspectedUser { get; set; }
        public UserListWithLoadMoreModel UsersModel { get; set; }
        public bool AjaxResult { get; set; }

        public async Task<IActionResult> OnGetAsync(string userName, [FromQuery] int offset = 0, [FromQuery] bool ajax = false)
        {
            var tuple = await FollowingsModelsBuilder.BuildUsersModelAsync(DbContext, userName, offset, UsersPortion, ajax, true);
            if (tuple == null)
            {
                return NotFound();
            }
            InspectedUser = tuple.Item1;
            UsersModel = tuple.Item2;
            AjaxResult = ajax;
            return Page();
        }

        //public static async Task<Tuple<ApplicationUser, UserListWithLoadMoreModel>> BuildUsersModelAsync(ApplicationDbContext dbContext,
        //        string userName, int offset, int count)
        //{
        //    var query = $"SELECT * FROM AspNetUsers WHERE NormalizedUserName='{userName}'";
        //    var user = await dbContext.Users.FromSql(query).FirstOrDefaultAsync();
        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    offset = Math.Max(offset, 0);
        //    var overflow = count + offset + 1 < 0;
        //    count = Math.Min(count, int.MaxValue - offset - 1);

        //    query = "SELECT * FROM AspNetUsers WHERE Id IN " +
        //                $"(SELECT FollowerId FROM Followers WHERE FollowerId='{user.Id}') " +
        //                "ORDER BY Id " +
        //                $"OFFSET {offset} ROWS " +
        //                $"FETCH NEXT {count + 1} ROWS ONLY";
        //    var users = await dbContext.ApplicationUser.FromSql(query).ToListAsync();

        //    LoadMoreModel loadMoreModel = null;
        //    if (users.Count > count && !overflow)
        //    {
        //        loadMoreModel = new LoadMoreModel()
        //        {
        //            Action = "/Followings/" + userName,
        //            Offset = offset + count,
        //        };
        //        users.RemoveAt(users.Count - 1);
        //    }

        //    var userList = new UserListWithLoadMoreModel()
        //    {
        //        Users = users,
        //        LoadMoreModel = loadMoreModel,
        //        DefaultText = "No followings yet",
        //    };
        //    return Tuple.Create(user, userList);
        //}

        public static int UsersPortion => 1;
    }
}