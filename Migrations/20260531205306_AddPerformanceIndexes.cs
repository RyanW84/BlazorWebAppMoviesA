using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorWebAppMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Movies_Director",
                table: "Movies",
                column: "Director");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_IsFavorite",
                table: "Movies",
                column: "IsFavorite");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Title",
                table: "Movies",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TmdbId",
                table: "Movies",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastMembers_Name",
                table: "CastMembers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CastMembers_TmdbPersonId",
                table: "CastMembers",
                column: "TmdbPersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movies_Director",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_IsFavorite",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_Title",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_TmdbId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_CastMembers_Name",
                table: "CastMembers");

            migrationBuilder.DropIndex(
                name: "IX_CastMembers_TmdbPersonId",
                table: "CastMembers");
        }
    }
}
