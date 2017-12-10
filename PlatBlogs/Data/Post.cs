using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using PlatBlogs.Interfaces;

namespace PlatBlogs.Data
{
    public class Post: IPost
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }

        public IList<Like> Likes { get; set; } = new List<Like>();

        IAuthor IPost.Author => Author;
    }
}
