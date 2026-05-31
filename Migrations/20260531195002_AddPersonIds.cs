using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorWebAppMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DirectorTmdbId",
                table: "Movies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TmdbPersonId",
                table: "CastMembers",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DirectorTmdbId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TmdbPersonId",
                table: "CastMembers");
        }
    }
}
