using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Data
{
    public class Like
    {
        public string LikerId { get; set; }
        public ApplicationUser Liker { get; set; }

        public string LikedUserId { get; set; }
        public int LikedPostId { get; set; }
        public Post Post { get; set; }
    }
}
