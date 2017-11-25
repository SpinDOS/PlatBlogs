using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatBlogs.Data;

namespace PlatBlogs.Pages._Partials
{
    public class PostView
    {
        public Post Post { get; set; }
        public bool Liked { get; set; }
        public int LikesCount { get; set; }
        public string ReturnUrl { get; set; }
    }
}
