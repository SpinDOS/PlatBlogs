using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlatBlogs.Data;

namespace PlatBlogs.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<FollowPair>()
                .HasKey(pair => new {pair.FollowedName, pair.FollowerName});

            builder.Entity<FollowPair>()
                .HasOne(pair => pair.Followed)
                .WithMany(user => user.Followers)
                .HasForeignKey(pair => pair.FollowedName);

            builder.Entity<FollowPair>()
                .HasOne(pair => pair.Follower)
                .WithMany(user => user.Following)
                .HasForeignKey(pair => pair.FollowerName)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<PlatBlogs.Data.ApplicationUser> ApplicationUser { get; set; }
    }
}
