using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class FixPart4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodIngredient",
                table: "FoodIngredient");

            migrationBuilder.DropIndex(
                name: "IX_FoodIngredient_FoodId",
                table: "FoodIngredient");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FoodIngredient");

            migrationBuilder.AddColumn<bool>(
                name: "IsVego",
                table: "DayFood",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodIngredient",
                table: "FoodIngredient",
                columns: new[] { "FoodId", "IngredientId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodIngredient",
                table: "FoodIngredient");

            migrationBuilder.DropColumn(
                name: "IsVego",
                table: "DayFood");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "FoodIngredient",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodIngredient",
                table: "FoodIngredient",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FoodIngredient_FoodId",
                table: "FoodIngredient",
                column: "FoodId");
        }
    }
}
