using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PlatBlogs.Data.Migrations
{
    public partial class AddNewPoststoredprocedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"CREATE PROCEDURE NewPost 
	@authorUserName nvarchar(450),
	@text nvarchar(450)
AS
	DECLARE @authorId nvarchar(450);
	SELECT @authorId = Id FROM AspNetUsers 
	WHERE NormalizedUserName = UPPER(@authorUserName);
	if @authorId IS NULL
		RETURN;

	DECLARE @PostMaxId int;
	SELECT @PostMaxId = MAX(Id) FROM Posts WHERE AuthorId = @authorId;
	INSERT INTO Posts(AuthorId, Id, DateTime, Message) 
		VALUES (@authorId, ISNULL(@PostMaxId, 0) + 1, GETDATE(), @text); ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE NewPost");
        }
    }
}
