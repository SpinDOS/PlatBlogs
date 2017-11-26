using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatBlogs.Data;

namespace PlatBlogs.Pages._Partials
{
    public class UserLeftMenu
    {
        public ApplicationUser User { get; set; }
        public int PostCount { get; set; }
        public int FollowingsCount { get; set; }
        public int FollowersCount { get; set; }
        public bool? Followed { get; set; }
    }
}
