using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PlatBlogs.Data.Migrations
{
    public partial class AddGetUserLeftMenuInfostoredprocedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"CREATE PROCEDURE GetUserLeftMenuInfo
	@userName nvarchar(450),
	@viewerUserName nvarchar(450),

    @postsCount int OUTPUT, 
    @followingsCount int OUTPUT, 
    @followersCount int OUTPUT, 

    @viewerId nvarchar(450) OUTPUT,
    @followedByViewer bit OUTPUT
AS
    DECLARE @normalizedUserName nvarchar(450);
    SET @normalizedUserName = UPPER(@userName);

	DECLARE @userId nvarchar(450);
    SELECT @userId = Id FROM AspNetUsers WHERE NormalizedUserName = @normalizedUserName;
    IF @userId IS NULL
        RETURN;

    SELECT @postsCount = COUNT(*) FROM Posts WHERE AuthorId = @userId;
    SELECT @followingsCount = COUNT(*) FROM Followers WHERE FollowerId = @userId;
	SELECT @followersCount = COUNT(*) FROM Followers WHERE FollowedId = @userId;

    DECLARE @normalizedViewerName nvarchar(450);
    SET @normalizedViewerName = UPPER(@viewerUserName);
    IF @normalizedViewerName = @normalizedUserName
    BEGIN
        SET @viewerId = @userId;
        SET @followedByViewer = NULL;
    END
    ELSE 
    BEGIN
        SELECT @viewerId = Id FROM AspNetUsers WHERE NormalizedUserName = @normalizedViewerName;
        SELECT @followedByViewer = COUNT(*) FROM Followers 
            WHERE FollowedId = @userId AND FollowerId = @viewerId;
    END

    SELECT Id, FullName, UserName, DateOfBirth, City, ShortInfo, PublicProfile, AvatarPath
    FROM AspNetUsers
    WHERE Id = @userId; ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE GetUserLeftMenuInfo");
        }
    }
}
