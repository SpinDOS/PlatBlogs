using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Views._Partials
{
    public class PostView : IRenderable
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
                    var post = new Post()
                    {
                        AuthorId = reader.GetString(0),
                        Id = reader.GetInt32(1),
                        DateTime = reader.GetDateTime(2),
                        Message = reader.GetString(3),
                    };
                    var postView = new PostView()
                    {
                        Post = post,
                        LikesCount = reader.GetInt32(4),
                        Liked = reader.GetInt32(5) == 1,
                        AuthorFullName = reader.GetString(6),
                        AuthorUserName = reader.GetString(7),
                        AuthorPublicProfile = reader.GetBoolean(8),
                    };
                    result.Add(postView);
                }
            }
            while (await reader.NextResultAsync());
            return result;
        }

        public async Task RenderAsync(IHtmlHelper iHtmlHelper)
        {
            await iHtmlHelper.PartialAsync("~/Views/_Partials/PostView.cshtml", this);
        }
    }
}
