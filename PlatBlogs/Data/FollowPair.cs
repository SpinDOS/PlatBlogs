using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Data
{
    public class FollowPair
    {
        public string FollowerName { get; set; }
        public ApplicationUser Follower { get; set; }

        public string FollowedName { get; set; }
        public ApplicationUser Followed { get; set; }
    }
}
