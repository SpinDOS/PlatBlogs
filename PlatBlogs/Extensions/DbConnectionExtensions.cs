using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatBlogs.Data;
using PlatBlogs.Interfaces;
using PlatBlogs.Views._Partials;

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
                cmd.CommandText = 
                    "SELECT Id FROM AspNetUsers WHERE NormalizedUserName = @normalizedUserName; ";
                return await cmd.ExecuteScalarAsync() as string;
            }
        }

        public static async Task<bool> CheckFollowingAsync(this DbConnection conn, string followedId, string followerId)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = 
                    $"SELECT 1 FROM Followers WHERE followedId='{followedId}' AND followerId='{followerId}'";
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

        public static async Task<bool> IsOpenedForViewerAsync(this DbConnection conn, IAuthor viewedUser, string viewerId)
        {
            return viewedUser.PublicProfile || viewedUser.Id == viewerId ||
                   await conn.CheckFollowingAsync(viewerId, viewedUser.Id);
        }

        public static async Task<ListWithLoadMoreModel> ReadToListWithLoadMoreModel<T>(this DbDataReader reader,
            int offset, int count,
            Func<DbDataReader, Task<IList<T>>> itemsReader,
            Func<LoadMoreModel> loadMoreModelBuilder = null) where T : IRenderable
        {
            if (!reader.HasRows)
            {
                return offset != 0 ? null : new ListWithLoadMoreModel();
            }

            ListWithLoadMoreModel result = new ListWithLoadMoreModel();

            var items = await itemsReader(reader);
            if (items.Count == count + 1)
            {
                items.RemoveAt(count);
                if (loadMoreModelBuilder != null)
                {
                    var loadMoreModel = loadMoreModelBuilder();
                    loadMoreModel.Offset = offset + count;
                    result.LoadMoreModel = loadMoreModel;
                }
            }
            result.Elements = items.Cast<IRenderable>();
            return result;
        }

    }
}
