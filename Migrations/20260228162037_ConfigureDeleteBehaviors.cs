using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHub.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureDeleteBehaviors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookGenres_Genres_GenreId",
                table: "BookGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "Context",
                table: "Reviews",
                newName: "Content");

            migrationBuilder.AddForeignKey(
                name: "FK_BookGenres_Genres_GenreId",
                table: "BookGenres",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookGenres_Genres_GenreId",
                table: "BookGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Reviews",
                newName: "Context");

            migrationBuilder.AddForeignKey(
                name: "FK_BookGenres_Genres_GenreId",
                table: "BookGenres",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
