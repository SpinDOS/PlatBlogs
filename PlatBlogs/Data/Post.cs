using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Data
{
    public class Post
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }
}
