using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class AddChores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChoreModel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedPerson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoreModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChoreStreak",
                columns: table => new
                {
                    ChoreId = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Person = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Streak = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoreStreak", x => new { x.ChoreId, x.Person });
                    table.ForeignKey(
                        name: "FK_ChoreStreak_ChoreModel_ChoreId",
                        column: x => x.ChoreId,
                        principalTable: "ChoreModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChoreStreak");

            migrationBuilder.DropTable(
                name: "ChoreModel");
        }
    }
}
