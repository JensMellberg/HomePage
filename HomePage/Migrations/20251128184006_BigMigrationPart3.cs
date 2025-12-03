using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class BigMigrationPart3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage");

            migrationBuilder.AlterColumn<Guid>(
                name: "IngredientId",
                table: "FoodStorage",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage",
                column: "IngredientId");

            migrationBuilder.CreateTable(
                name: "SpendingGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Patterns = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IgnoreTowardsTotal = table.Column<bool>(type: "bit", nullable: false),
                    Person = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpendingItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Person = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Place = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    SetGroupId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingItem", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpendingGroup");

            migrationBuilder.DropTable(
                name: "SpendingItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage");

            migrationBuilder.AlterColumn<string>(
                name: "IngredientId",
                table: "FoodStorage",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage",
                column: "IngredientId");
        }
    }
}
