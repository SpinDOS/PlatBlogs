using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Views._Partials
{
    public class PostView
    {
        public Post Post { get; set; }
        public string AuthorFullName { get; set; }
        public string AuthorUserName { get; set; }
        public bool AuthorPublicProfile { get; set; }
        public bool Liked { get; set; }
        public int LikesCount { get; set; }
        
        public static async Task<List<PostView>> FromSqlReaderAsync(DbDataReader reader)
        {
            var result = new List<PostView>();
            do
            {
                while (await reader.ReadAsync())
                {
                    var authorId = reader.GetString(0);
                    var postId = reader.GetInt32(1);
                    var postTime = reader.GetDateTime(2);
                    var message = reader.GetString(3);
                    var likesCount = reader.GetInt32(4);
                    var liked = reader.GetInt32(5) == 1;
                    var authorFullName = reader.GetString(6);
                    var authorUserName = reader.GetString(7);
                    var authorPublicProfile = reader.GetBoolean(8);

                    var post = new Post()
                    {
                        AuthorId = authorId,
                        Id = postId,
                        DateTime = postTime,
                        Message = message,
                    };
                    var postView = new PostView()
                    {
                        Post = post,
                        LikesCount = likesCount,
                        Liked = liked,
                        AuthorFullName = authorFullName,
                        AuthorUserName = authorUserName,
                        AuthorPublicProfile = authorPublicProfile,
                    };
                    result.Add(postView);
                }
            }
            while (await reader.NextResultAsync());
            return result;
        }
    }
}
