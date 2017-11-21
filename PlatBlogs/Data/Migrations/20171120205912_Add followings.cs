using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PlatBlogs.Data.Migrations
{
    public partial class Addfollowings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowPair",
                columns: table => new
                {
                    FollowedName = table.Column<string>(nullable: false),
                    FollowerName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowPair", x => new { x.FollowedName, x.FollowerName });
                    table.ForeignKey(
                        name: "FK_FollowPair_AspNetUsers_FollowedName",
                        column: x => x.FollowedName,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FollowPair_AspNetUsers_FollowerName",
                        column: x => x.FollowerName,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowPair_FollowerName",
                table: "FollowPair",
                column: "FollowerName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowPair");
        }
    }
}
