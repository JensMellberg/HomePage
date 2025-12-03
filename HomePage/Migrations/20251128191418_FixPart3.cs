using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class FixPart3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "FoodStorage",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FoodStorage_IngredientId",
                table: "FoodStorage",
                column: "IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodStorage_Ingredient_IngredientId",
                table: "FoodStorage",
                column: "IngredientId",
                principalTable: "Ingredient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodStorage_Ingredient_IngredientId",
                table: "FoodStorage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage");

            migrationBuilder.DropIndex(
                name: "IX_FoodStorage_IngredientId",
                table: "FoodStorage");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FoodStorage");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodStorage",
                table: "FoodStorage",
                column: "IngredientId");
        }
    }
}
