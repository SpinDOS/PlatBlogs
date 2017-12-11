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
using PlatBlogs.Interfaces;

namespace PlatBlogs.Views._Partials
{
    public class PostViewModel : IPost, IRenderable
    {
        public PostViewModel() { }

        public PostViewModel(Post post, bool liked, int likesCount)
        {
            this.Author = post.Author;
            this.Id = post.Id;
            this.Message = post.Message;
            this.DateTime = post.DateTime;
            this.Liked = liked;
            this.LikesCount = likesCount;
        }
        public IAuthor Author { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }

        public bool Liked { get; set; }
        public int LikesCount { get; set; }
        
        public static async Task<IList<PostViewModel>> FromSqlReaderAsync(DbDataReader reader)
        {
            var result = new List<PostViewModel>();
            do
            {
                while (await reader.ReadAsync())
                {
                    var author = new UserViewModel() { Id = reader.GetString(0) };
                    var postView = new PostViewModel()
                    {
                        Author = author,
                        Id = reader.GetInt32(1),
                        DateTime = reader.GetDateTime(2),
                        Message = reader.GetString(3),
                        LikesCount = reader.GetInt32(4),
                        Liked = reader.GetInt32(5) == 1,
                    };
                    if (reader.FieldCount > 8)
                    {
                        author.FullName = reader.GetString(6);
                        author.UserName = reader.GetString(7);
                        author.PublicProfile = reader.GetBoolean(8);
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
