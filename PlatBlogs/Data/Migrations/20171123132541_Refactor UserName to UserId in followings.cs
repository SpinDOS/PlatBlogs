using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PlatBlogs.Data.Migrations
{
    public partial class RefactorUserNametoUserIdinfollowings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FollowPair_AspNetUsers_FollowedName",
                table: "FollowPair");

            migrationBuilder.DropForeignKey(
                name: "FK_FollowPair_AspNetUsers_FollowerName",
                table: "FollowPair");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FollowPair",
                table: "FollowPair");

            migrationBuilder.RenameTable(
                name: "FollowPair",
                newName: "Followers");

            migrationBuilder.RenameColumn(
                name: "FollowerName",
                table: "Followers",
                newName: "FollowerId");

            migrationBuilder.RenameColumn(
                name: "FollowedName",
                table: "Followers",
                newName: "FollowedId");

            migrationBuilder.RenameIndex(
                name: "IX_FollowPair_FollowerName",
                table: "Followers",
                newName: "IX_Followers_FollowerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Followers",
                table: "Followers",
                columns: new[] { "FollowedId", "FollowerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Followers_AspNetUsers_FollowedId",
                table: "Followers",
                column: "FollowedId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Followers_AspNetUsers_FollowerId",
                table: "Followers",
                column: "FollowerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Followers_AspNetUsers_FollowedId",
                table: "Followers");

            migrationBuilder.DropForeignKey(
                name: "FK_Followers_AspNetUsers_FollowerId",
                table: "Followers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Followers",
                table: "Followers");

            migrationBuilder.RenameTable(
                name: "Followers",
                newName: "FollowPair");

            migrationBuilder.RenameColumn(
                name: "FollowerId",
                table: "FollowPair",
                newName: "FollowerName");

            migrationBuilder.RenameColumn(
                name: "FollowedId",
                table: "FollowPair",
                newName: "FollowedName");

            migrationBuilder.RenameIndex(
                name: "IX_Followers_FollowerId",
                table: "FollowPair",
                newName: "IX_FollowPair_FollowerName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FollowPair",
                table: "FollowPair",
                columns: new[] { "FollowedName", "FollowerName" });

            migrationBuilder.AddForeignKey(
                name: "FK_FollowPair_AspNetUsers_FollowedName",
                table: "FollowPair",
                column: "FollowedName",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FollowPair_AspNetUsers_FollowerName",
                table: "FollowPair",
                column: "FollowerName",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
