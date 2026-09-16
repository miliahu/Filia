using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Filia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileTitleAndDescriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "filia",
                table: "files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileTitle",
                schema: "filia",
                table: "files",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "filia",
                table: "files");

            migrationBuilder.DropColumn(
                name: "FileTitle",
                schema: "filia",
                table: "files");
        }
    }
}
