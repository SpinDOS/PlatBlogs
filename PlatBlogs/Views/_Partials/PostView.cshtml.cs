using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlatBlogs.Data;
using PlatBlogs.Extensions;

namespace PlatBlogs.Views._Partials
{
    public class PostViewModel : IRenderable
    {
        public Post Post { get; set; }
        public IAuthor Author { get; set; }
        public bool Liked { get; set; }
        public int LikesCount { get; set; }
        
        public static async Task<List<PostViewModel>> FromSqlReaderAsync(DbDataReader reader)
        {
            var result = new List<PostViewModel>();
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
                    var postView = new PostViewModel()
                    {
                        Post = post,
                        LikesCount = reader.GetInt32(4),
                        Liked = reader.GetInt32(5) == 1,
                    };
                    if (reader.FieldCount > 8)
                    {
                        postView.Author = new SimpleAuthor
                        { 
                            FullName = reader.GetString(6),
                            UserName = reader.GetString(7),
                            PublicProfile = reader.GetBoolean(8),
                        };
                    }
                    result.Add(postView);
                }
            }
            while (await reader.NextResultAsync());
            return result;
        }

        public async Task<object> RenderAsync(IHtmlHelper iHtmlHelper) 
            => await iHtmlHelper.PartialAsync("~/Views/_Partials/PostView.cshtml", this);
    }
}
