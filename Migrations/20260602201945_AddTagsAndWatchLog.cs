using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorWebAppMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsAndWatchLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WatchLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    WatchedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieTags_MovieId_Tag",
                table: "MovieTags",
                columns: new[] { "MovieId", "Tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieTags_Tag",
                table: "MovieTags",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLog_MovieId",
                table: "WatchLog",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLog_WatchedOn",
                table: "WatchLog",
                column: "WatchedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieTags");

            migrationBuilder.DropTable(
                name: "WatchLog");
        }
    }
}
