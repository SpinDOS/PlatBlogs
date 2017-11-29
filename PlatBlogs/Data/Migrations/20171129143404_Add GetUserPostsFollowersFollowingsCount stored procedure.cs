using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PlatBlogs.Data.Migrations
{
    public partial class AddGetUserPostsFollowersFollowingsCountstoredprocedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"CREATE PROCEDURE GetUserPostsFollowersFollowingsCount
	@userId NVARCHAR(450),
    @postsCount INT OUTPUT, 
    @followingsCount INT OUTPUT, 
    @followersCount INT OUTPUT
AS
    SELECT @postsCount = COUNT(*) FROM Posts
    WHERE AuthorId = @userId;
    SELECT @followingsCount = COUNT(*) FROM Followers
    WHERE FollowerId = @userId;
	SELECT @followersCount = COUNT(*) FROM Followers
    WHERE FollowedId = @userId;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE GetUserPostsFollowersFollowingsCount;");
        }
    }
}
