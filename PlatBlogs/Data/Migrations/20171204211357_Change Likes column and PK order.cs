using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PlatBlogs.Data.Migrations
{
    public partial class ChangeLikescolumnandPKorder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"CREATE TABLE #MyTemp (
    [LikerId]     NVARCHAR (450) NOT NULL,
    [LikedUserId] NVARCHAR (450) NOT NULL,
    [LikedPostId] INT            NOT NULL
);
Go

INSERT INTO #MyTemp (LikerId, LikedUserId, LikedPostId)
SELECT LikerId, LikedUserId, LikedPostId From [dbo].[Likes];

DROP TABLE [dbo].[Likes];
GO

CREATE TABLE [dbo].[Likes] (
    [LikedUserId] NVARCHAR (450) NOT NULL,
    [LikedPostId] INT            NOT NULL,
    [LikerId]     NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_Likes] PRIMARY KEY CLUSTERED ([LikedPostId] ASC, [LikerId] ASC, [LikedUserId] ASC),
    CONSTRAINT [FK_Likes_Posts_LikedUserId_LikedPostId] FOREIGN KEY ([LikedUserId], [LikedPostId]) 
        REFERENCES [dbo].[Posts] ([AuthorId], [Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Likes_AspNetUsers_LikerId] FOREIGN KEY ([LikerId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_Likes_LikedUserId_LikedPostId]
    ON [dbo].[Likes]([LikedUserId] ASC, [LikedPostId] ASC);

INSERT INTO [dbo].[Likes] (LikerId, LikedUserId, LikedPostId)
SELECT * From #MyTemp;

DROP TABLE #MyTemp;
GO
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"CREATE TABLE #MyTemp (
    [LikerId]     NVARCHAR (450) NOT NULL,
    [LikedUserId] NVARCHAR (450) NOT NULL,
    [LikedPostId] INT            NOT NULL
);
Go

INSERT INTO #MyTemp (LikerId, LikedUserId, LikedPostId)
SELECT LikerId, LikedUserId, LikedPostId From [dbo].[Likes];

DROP TABLE [dbo].[Likes];
GO

CREATE TABLE [dbo].[Likes] (
    [LikerId]     NVARCHAR (450) NOT NULL,
    [LikedUserId] NVARCHAR (450) NOT NULL,
    [LikedPostId] INT            NOT NULL,
    CONSTRAINT [PK_Likes] PRIMARY KEY CLUSTERED ([LikedUserId] ASC, [LikedPostId] ASC, [LikerId] ASC),
    CONSTRAINT [FK_Likes_AspNetUsers_LikerId] FOREIGN KEY ([LikerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Likes_Posts_LikedUserId_LikedPostId] FOREIGN KEY ([LikedUserId], [LikedPostId]) REFERENCES [dbo].[Posts] ([AuthorId], [Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_Likes_LikedUserId_LikedPostId]
    ON [dbo].[Likes]([LikedUserId] ASC, [LikedPostId] ASC);

INSERT INTO [dbo].[Likes] (LikerId, LikedUserId, LikedPostId)
SELECT * From #MyTemp;

DROP TABLE #MyTemp;
GO
");
        }
    }
}
