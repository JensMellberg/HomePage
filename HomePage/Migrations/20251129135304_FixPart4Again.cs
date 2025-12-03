using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class FixPart4Again : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayFood_Food_FoodId",
                table: "DayFood");

            migrationBuilder.RenameColumn(
                name: "FoodId",
                table: "DayFood",
                newName: "MainFoodId");

            migrationBuilder.RenameIndex(
                name: "IX_DayFood_FoodId",
                table: "DayFood",
                newName: "IX_DayFood_MainFoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayFood_Food_MainFoodId",
                table: "DayFood",
                column: "MainFoodId",
                principalTable: "Food",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayFood_Food_MainFoodId",
                table: "DayFood");

            migrationBuilder.RenameColumn(
                name: "MainFoodId",
                table: "DayFood",
                newName: "FoodId");

            migrationBuilder.RenameIndex(
                name: "IX_DayFood_MainFoodId",
                table: "DayFood",
                newName: "IX_DayFood_FoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayFood_Food_FoodId",
                table: "DayFood",
                column: "FoodId",
                principalTable: "Food",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
