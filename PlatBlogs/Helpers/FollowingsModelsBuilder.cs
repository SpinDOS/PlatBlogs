﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlatBlogs.Data;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Helpers
{
    public static class FollowingsModelsBuilder
    {
        // Following and Followers pages are very similar, so 
        // this class implements logic for both
        public static async Task<Tuple<ApplicationUser, UserListWithLoadMoreModel>> BuildUsersModelAsync(ApplicationDbContext dbContext,
            string userName, int offset, int count, bool ajax, bool followINGModel)
        {
            var followEnding = followINGModel ? "ing" : "er";

            var query = $"SELECT * FROM AspNetUsers WHERE NormalizedUserName='{userName}'";
            var user = await dbContext.Users.FromSql(query).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            var overflow = OffsetCountResolver.ResolveOffsetCount(ref offset, ref count, ajax);

            query = "SELECT * FROM AspNetUsers WHERE Id IN " +
                    $"(SELECT Followe{(followINGModel ? "d" : "r")}Id FROM Followers " +
                    $"WHERE Followe{(followINGModel? "r" : "d")}Id='{user.Id}') " +
                    "ORDER BY UserName " +
                    $"OFFSET {offset} ROWS " +
                    $"FETCH NEXT {count + 1} ROWS ONLY";
            var users = await dbContext.ApplicationUser.FromSql(query).ToListAsync();

            LoadMoreModel loadMoreModel = null;
            var moreUsersExist = users.Count > count && !overflow;
            if (moreUsersExist)
            {
                loadMoreModel = new LoadMoreModel()
                {
                    Action = $"/Follow{followEnding}s/" + userName,
                    Offset = offset + count,
                };
                users.RemoveAt(users.Count - 1);
            }

            var userList = new UserListWithLoadMoreModel()
            {
                Users = users,
                DefaultText = $"No follow{followEnding}s yet",
                LoadMoreModel = loadMoreModel,
                MoreUsersExist = moreUsersExist,
            };
            return Tuple.Create(user, userList);
        }
    }
}