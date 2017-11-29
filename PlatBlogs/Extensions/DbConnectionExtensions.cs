using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Extensions
{
    public static class DbConnectionExtensions
    {
        public static void AddWithValue(this IDataParameterCollection parameters, string name, string value)
        {
            parameters.Add(new SqlParameter(name, value));
        }

        public static async Task<string> GetUserIdByNameAsync(this DbConnection conn, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;
            using (var cmd = conn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("normalizedUserName", userName.ToUpper());
                cmd.CommandText = "SELECT Id FROM AspNetUsers " +
                                  "WHERE NormalizedUserName=@normalizedUserName";
                return await cmd.ExecuteScalarAsync() as string;
            }
        }

        public static async Task<bool> CheckFollowingAsync(this DbConnection conn, string followedId, string followerId)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT 'NOTUSED' FROM Followers " +
                                  $"WHERE followedId='{followedId}' AND followerId='{followerId}'";
                return await cmd.ExecuteScalarAsync() != null;
            }
        }

        public static async Task<bool> IsOpenedForViewerAsync(this DbConnection conn, string viewedId, string viewerId)
        {
            if (viewedId == viewerId)
                return true;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT PublicProfile FROM AspNetUsers WHERE Id='{viewedId}'";
                if ((bool) await cmd.ExecuteScalarAsync())
                    return true;
            }
            return await CheckFollowingAsync(conn, viewerId, viewedId);
        }

    }
}
