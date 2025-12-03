using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class BigMigrationPart1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ToDo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CalendarActivity",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Person = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CalendarDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInDays = table.Column<int>(type: "int", nullable: false),
                    IsReoccuring = table.Column<bool>(type: "bit", nullable: false),
                    IsVacation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarActivity", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GoalPerWeek = table.Column<int>(type: "int", nullable: false),
                    IsBad = table.Column<bool>(type: "bit", nullable: false),
                    NeedsOnAllSides = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CurrentWordMix",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Letters = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentWordMix", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "ExtraWord",
                columns: table => new
                {
                    Word = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Creator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JensApproved = table.Column<bool>(type: "bit", nullable: false),
                    AnnaApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraWord", x => x.Word);
                });

            migrationBuilder.CreateTable(
                name: "RedDay",
                columns: table => new
                {
                    DayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedDay", x => x.DayName);
                });

            migrationBuilder.CreateTable(
                name: "ThemeDay",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThemeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeDay", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarActivity");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "CurrentWordMix");

            migrationBuilder.DropTable(
                name: "ExtraWord");

            migrationBuilder.DropTable(
                name: "RedDay");

            migrationBuilder.DropTable(
                name: "ThemeDay");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ToDo",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
