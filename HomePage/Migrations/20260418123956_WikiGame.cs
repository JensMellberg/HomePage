using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class WikiGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachedWikiGamePages",
                columns: table => new
                {
                    Title = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PageContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedLinks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedWikiGamePages", x => x.Title);
                });

            migrationBuilder.CreateTable(
                name: "WikiGameNavigations",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Step = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiGameNavigations", x => new { x.Date, x.UserName, x.Step });
                });

            migrationBuilder.CreateTable(
                name: "WikiGameStarts",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiGameStarts", x => x.Date);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedWikiGamePages");

            migrationBuilder.DropTable(
                name: "WikiGameNavigations");

            migrationBuilder.DropTable(
                name: "WikiGameStarts");
        }
    }
}
