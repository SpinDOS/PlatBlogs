using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Data
{
    public class FollowPair
    {
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        public string FollowedId { get; set; }
        public ApplicationUser Followed { get; set; }
    }
}
