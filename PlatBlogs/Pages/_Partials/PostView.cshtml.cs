using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Pages._Partials
{
    public class PostView
    {
        public Post Post { get; set; }
        public bool Liked { get; set; }
        public int LikesCount { get; set; }

        public static async Task<PostView> FromPost(Post post, DbConnection conn, ClaimsPrincipal currentUser)
        {
            var result = new PostView() { Post = post };
            var myId = await conn.GetUserIdByNameAsync(currentUser.Identity.Name);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Likes WHERE " +
                                  $"LikerId='{myId}' AND LikedUserId='{post.AuthorId}' AND LikedPostId='{post.Id}'";
                result.Liked = await cmd.ExecuteScalarAsync() != null;
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM Likes WHERE LikedUserId='{post.AuthorId}' AND LikedPostId='{post.Id}'";
                result.LikesCount = (int) await cmd.ExecuteScalarAsync();
            }

            return result;
        }
    }
}
