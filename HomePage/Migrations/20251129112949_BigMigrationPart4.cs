using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class BigMigrationPart4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Food",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsSideDish = table.Column<bool>(type: "bit", nullable: false),
                    RecipeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InFolder = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Food", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DayFood",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Portions = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayFood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayFood_Food_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Food",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodCategories",
                columns: table => new
                {
                    CategoriesKey = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodCategories", x => new { x.CategoriesKey, x.FoodId });
                    table.ForeignKey(
                        name: "FK_FoodCategories_Category_CategoriesKey",
                        column: x => x.CategoriesKey,
                        principalTable: "Category",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodCategories_Food_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Food",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodIngredient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodIngredient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodIngredient_Food_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Food",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodIngredient_Ingredient_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayFoodSideDishes",
                columns: table => new
                {
                    DayFoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SideDishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayFoodSideDishes", x => new { x.DayFoodId, x.SideDishId });
                    table.ForeignKey(
                        name: "FK_DayFoodSideDishes_DayFood_DayFoodId",
                        column: x => x.DayFoodId,
                        principalTable: "DayFood",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayFoodSideDishes_Food_SideDishId",
                        column: x => x.SideDishId,
                        principalTable: "Food",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayFood_FoodId",
                table: "DayFood",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_DayFoodSideDishes_SideDishesId",
                table: "DayFoodSideDishes",
                column: "SideDishId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_FoodId",
                table: "FoodCategories",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodIngredient_FoodId",
                table: "FoodIngredient",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodIngredient_IngredientId",
                table: "FoodIngredient",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayFoodSideDishes");

            migrationBuilder.DropTable(
                name: "FoodCategories");

            migrationBuilder.DropTable(
                name: "FoodIngredient");

            migrationBuilder.DropTable(
                name: "DayFood");

            migrationBuilder.DropTable(
                name: "Food");
        }
    }
}
