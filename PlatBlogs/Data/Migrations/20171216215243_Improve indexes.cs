using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PlatBlogs.Data.Migrations
{
    public partial class Improveindexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX [IX_Likes_LikedUserId_LikedPostId] ON [dbo].[Likes];

CREATE NONCLUSTERED INDEX [IX_Likes_LikerId] ON [dbo].[Likes]
(
	[LikerId] ASC
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX [IX_Likes_LikerId] ON [dbo].[Likes];

CREATE NONCLUSTERED INDEX [IX_Likes_LikedUserId_LikedPostId] ON [dbo].[Likes]
(
	[LikedUserId] ASC,
	[LikedPostId] ASC
);
");
        }
    }
}
