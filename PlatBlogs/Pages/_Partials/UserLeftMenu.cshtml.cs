using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

            if (user.UserName != currentUser.Identity.Name)
                result.Followed = await conn.CheckFollowingAsync(user.Id, currentUserId);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM Posts WHERE AuthorId='{user.Id}'";
                result.PostCount = (int) await cmd.ExecuteScalarAsync();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM Followers WHERE FollowedId='{user.Id}'";
                result.FollowersCount = (int) await cmd.ExecuteScalarAsync();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM Followers WHERE FollowerId='{user.Id}'";
                result.FollowingsCount = (int) await cmd.ExecuteScalarAsync();
            }
            return result;
        }
    }
}
