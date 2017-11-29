using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatBlogs.Data;
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

        public static async Task<List<PostView>> SimpleQueryPosts(
            this DbConnection conn, string viewerId,
            Func<ISimpleQueryPostsFieldNames, string> where = null,
            Func<ISimpleQueryPostsFieldNames, string> orderBy = null,
            int offset = 0, int? count = null)
        {
            var stringBuilder = new StringBuilder(
$@"SELECT P.*, 
{_simpleQueryPostsFieldNames.LikesCount}, 
{_simpleQueryPostsFieldNames.MyLikeCount}, 
U.FullName, U.UserName, U.PublicProfile
FROM Posts P 
LEFT JOIN AspNetUsers U ON P.AuthorId = U.Id LEFT JOIN  
(SELECT LikedUserId, LikedPostId, COUNT(*) AS Count FROM Likes GROUP BY LikedUserId, LikedPostId) AllLikes
ON P.AuthorId = AllLikes.LikedUserId AND P.Id = AllLikes.LikedPostId
LEFT JOIN 
(SELECT LikedUserId, LikedPostId, COUNT(*) AS Count FROM Likes WHERE LikerId='{viewerId}' GROUP by LikedUserId, LikedPostId) MyLikes
ON P.AuthorId = MyLikes.LikedUserId AND P.Id = MyLikes.LikedPostId
{where?.Invoke(_simpleQueryPostsFieldNames)} 
");
            if (offset > 0 || count.HasValue)
            {
                var orderByString = orderBy?.Invoke(_simpleQueryPostsFieldNames);
                if (string.IsNullOrWhiteSpace(orderByString))
                    orderByString = "ORDER BY P.AuthorId, P.Id";
                stringBuilder.AppendLine(orderByString);

                stringBuilder.AppendLine($"OFFSET {offset} ROWS");
                if (count.HasValue)
                    stringBuilder.AppendLine($"FETCH NEXT {count.Value} ROWS ONLY");
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = stringBuilder.ToString();
                return await PostView.FromSqlReaderAsync(await cmd.ExecuteReaderAsync());
            }
        }

        public interface ISimpleQueryPostsFieldNames
        {
            string AuthorId { get; }
            string PostId { get; }
            string PostTime { get; }
            string Message { get; }
            string AuthorFullName { get; }
            string AuthorUserName { get; }
            string AuthorPublicProfile { get; }
            string LikesCount { get; }
            string MyLikeCount { get; }
        }

        private class SimpleQueryPostsFieldNames : ISimpleQueryPostsFieldNames
        {
            public string AuthorId => "P." + nameof(Data.Post.AuthorId);
            public string PostId => "P." + nameof(Data.Post.Id);
            public string PostTime => "P." + nameof(Data.Post.DateTime);
            public string Message => "P." + nameof(Data.Post.Message);
            public string AuthorFullName => "U." + nameof(ApplicationUser.FullName);
            public string AuthorUserName => "U." + nameof(ApplicationUser.UserName);
            public string AuthorPublicProfile => "U." + nameof(ApplicationUser.PublicProfile);
            public string LikesCount => "ISNULL(AllLikes.Count, 0)";
            public string MyLikeCount => "ISNULL(MyLikes.Count, 0)";
        }
        private static readonly ISimpleQueryPostsFieldNames _simpleQueryPostsFieldNames = new SimpleQueryPostsFieldNames();
    }
}
