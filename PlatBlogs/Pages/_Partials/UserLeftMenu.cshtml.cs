using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Pages._Partials
{
    public class UserLeftMenu
    {
        public ApplicationUser User { get; set; }
        public int PostCount { get; set; }
        public int FollowingsCount { get; set; }
        public int FollowersCount { get; set; }
        public bool? Followed { get; set; }

        public static async Task<UserLeftMenu> FromApplicationUser(ApplicationUser user, DbConnection conn, ClaimsPrincipal currentUser)
        {
            var result = new UserLeftMenu() {User = user};
            var currentUserId = await conn.GetUserIdByNameAsync(currentUser.Identity.Name);

            result.Followed = null;
            if (user.Id != currentUserId)
                result.Followed = await conn.CheckFollowingAsync(user.Id, currentUserId);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "GetUserPostsFollowersFollowingsCount";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", user.Id);
                cmd.Parameters.Add(new SqlParameter("@postsCount", SqlDbType.Int) {Direction = ParameterDirection.Output});
                cmd.Parameters.Add(new SqlParameter("@followingsCount", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new SqlParameter("@followersCount", SqlDbType.Int) { Direction = ParameterDirection.Output });
                await cmd.ExecuteNonQueryAsync();
                result.PostCount = (int) cmd.Parameters["@postsCount"].Value;
                result.FollowingsCount = (int) cmd.Parameters["@followingsCount"].Value;
                result.FollowersCount = (int) cmd.Parameters["@followersCount"].Value;
            }
            return result;
        }
    }
}
