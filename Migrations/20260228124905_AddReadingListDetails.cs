using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHub.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingListDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ReadingLists",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "ReadingLists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ReadingLists",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ReadingLists");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "ReadingLists");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ReadingLists");
        }
    }
}
