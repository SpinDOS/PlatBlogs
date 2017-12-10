using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PlatBlogs.Interfaces;

namespace PlatBlogs.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser, IUser
    {
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string City { get; set; }
        public string ShortInfo { get; set; }
        public string AvatarPath { get; set; }

        public bool PublicProfile { get; set; }

        public IList<FollowPair> Following { get; set; } = new List<FollowPair>();
        public IList<FollowPair> Followers { get; set; } = new List<FollowPair>();

        public IList<Post> Posts { get; set; } = new List<Post>();

        public IList<Like> Likes { get; set; } = new List<Like>();
    }
}
