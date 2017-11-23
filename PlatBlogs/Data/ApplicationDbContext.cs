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


            builder.Entity<Post>()
                .HasKey(post => new { post.AuthorId, post.Id });

            builder.Entity<Post>()
                .HasOne(post => post.Author)
                .WithMany(user => user.Posts)
                .HasForeignKey(post => post.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasKey(like => new { like.LikerId, like.LikedUserId, like.LikedPostId });

            builder.Entity<Like>()
                .HasOne(like => like.Liker)
                .WithMany(user => user.Likes)
                .HasForeignKey(like => like.LikerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => new { like.LikedUserId, like.LikedPostId })
                .OnDelete(DeleteBehavior.Restrict);

        }

        public DbSet<PlatBlogs.Data.ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
    }
}
