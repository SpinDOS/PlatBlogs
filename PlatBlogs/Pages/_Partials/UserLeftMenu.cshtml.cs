using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Pages._Partials
{
    public class UserLeftMenu
    {
        public UserLeftMenu(ApplicationUser user, IDbConnection conn, ClaimsPrincipal currentUser)
        {
            User = user;
            var currentUserId = DbConnectionExtensions.GetUserIdByName(conn, currentUser.Identity.Name);

            if (user.UserName != currentUser.Identity.Name)
                Followed = DbConnectionExtensions.CheckFollowing(conn, user.Id, currentUserId);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM Posts WHERE AuthorId='{user.Id}'";
                PostCount = (int) cmd.ExecuteScalar();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM Followers WHERE FollowedId='{user.Id}'";
                FollowersCount = (int) cmd.ExecuteScalar();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM Followers WHERE FollowerId='{user.Id}'";
                FollowingsCount = (int) cmd.ExecuteScalar();
            }
        }
        public ApplicationUser User { get; set; }
        public int PostCount { get; set; }
        public int FollowingsCount { get; set; }
        public int FollowersCount { get; set; }
        public bool? Followed { get; set; }
    }
}
