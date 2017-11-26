﻿using System;
using System.Collections.Generic;
using System.Data;
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
        public static string GetUserIdByName(IDbConnection conn, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;
            using (var cmd = conn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("userName", userName);
                cmd.CommandText = "SELECT Id FROM AspNetUsers " +
                                  "WHERE UserName=@userName";
                return cmd.ExecuteScalar() as string;
            }
        }

        public static bool CheckFollowing(IDbConnection conn, string followedId, string followerId)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT 'NOTUSED' FROM Followers " +
                                  $"WHERE followedId='{followedId}' AND followerId='{followerId}'";
                return cmd.ExecuteScalar() != null;
            }
        }

    }
}
