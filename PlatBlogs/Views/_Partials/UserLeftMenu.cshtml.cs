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

namespace PlatBlogs.Views._Partials
{
    public class UserLeftMenuModel: IUserBasicInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string City { get; set; }
        public string ShortInfo { get; set; }
        public bool PublicProfile { get; set; }
        private string _avatarPath;
        public string AvatarPath
        {
            get => _avatarPath ?? "/avatars/_no_image_.png";
            set => _avatarPath = value;
        }
        

        public int PostCount { get; set; }
        public int FollowingsCount { get; set; }
        public int FollowersCount { get; set; }

        public string ViewerId { get; set; }
        public bool? FollowedByViewer { get; set; }


        public static async Task<UserLeftMenuModel> FromDatabase(DbConnection conn, string userName, ClaimsPrincipal currentUser)
        {
            UserLeftMenuModel result = null;
            var viewerUserName = currentUser.Identity.Name;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "GetUserLeftMenuInfo";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userName", userName?? viewerUserName);
                cmd.Parameters.AddWithValue("@viewerUserName", viewerUserName);
                cmd.Parameters.Add(new SqlParameter("@postsCount", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new SqlParameter("@followingsCount", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new SqlParameter("@followersCount", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new SqlParameter("@viewerId", SqlDbType.NVarChar, 450) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new SqlParameter("@followedByViewer", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                        return null;
                    result = new UserLeftMenuModel
                    {
                        Id = reader.GetString(0),
                        FullName = reader.GetString(1),
                        UserName = reader.GetString(2),
                        DateOfBirth = reader.GetValue(3) as DateTime?,
                        City = reader.GetValue(4) as string,
                        ShortInfo = reader.GetValue(5) as string,
                        PublicProfile = reader.GetBoolean(6),
                        AvatarPath = reader.GetValue(7) as string,
                    };
                }
                result.PostCount = (int) cmd.Parameters["@postsCount"].Value;
                result.FollowingsCount = (int) cmd.Parameters["@followingsCount"].Value;
                result.FollowersCount = (int) cmd.Parameters["@followersCount"].Value;

                result.ViewerId = (string) cmd.Parameters["@viewerId"].Value;
                result.FollowedByViewer = cmd.Parameters["@followedByViewer"].Value as bool?;
            }
            return result;
        }
    }
}
