using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class FixPart4LastTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayFood_Food_MainFoodId",
                table: "DayFood");

            migrationBuilder.DropTable(
                name: "DayFoodSideDishes");

            migrationBuilder.DropIndex(
                name: "IX_DayFood_MainFoodId",
                table: "DayFood");

            migrationBuilder.DropColumn(
                name: "MainFoodId",
                table: "DayFood");

            migrationBuilder.CreateTable(
                name: "DayFoodDishConnection",
                columns: table => new
                {
                    DayFoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsMainDish = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayFoodDishConnection", x => new { x.DayFoodId, x.FoodId });
                    table.ForeignKey(
                        name: "FK_DayFoodDishConnection_DayFood_DayFoodId",
                        column: x => x.DayFoodId,
                        principalTable: "DayFood",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayFoodDishConnection_Food_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Food",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayFoodDishConnection_FoodId",
                table: "DayFoodDishConnection",
                column: "FoodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayFoodDishConnection");

            migrationBuilder.AddColumn<Guid>(
                name: "MainFoodId",
                table: "DayFood",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DayFoodSideDishes",
                columns: table => new
                {
                    SideDishInId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SideDishesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayFoodSideDishes", x => new { x.SideDishInId, x.SideDishesId });
                    table.ForeignKey(
                        name: "FK_DayFoodSideDishes_DayFood_SideDishInId",
                        column: x => x.SideDishInId,
                        principalTable: "DayFood",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayFoodSideDishes_Food_SideDishesId",
                        column: x => x.SideDishesId,
                        principalTable: "Food",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayFood_MainFoodId",
                table: "DayFood",
                column: "MainFoodId");

            migrationBuilder.CreateIndex(
                name: "IX_DayFoodSideDishes_SideDishesId",
                table: "DayFoodSideDishes",
                column: "SideDishesId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayFood_Food_MainFoodId",
                table: "DayFood",
                column: "MainFoodId",
                principalTable: "Food",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
