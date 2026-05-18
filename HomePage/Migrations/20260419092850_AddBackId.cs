using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class AddBackId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WikiGameNavigations",
                table: "WikiGameNavigations");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "WikiGameNavigations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "BackId",
                table: "WikiGameNavigations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WikiGameNavigations",
                table: "WikiGameNavigations",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WikiGameNavigations",
                table: "WikiGameNavigations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "WikiGameNavigations");

            migrationBuilder.DropColumn(
                name: "BackId",
                table: "WikiGameNavigations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WikiGameNavigations",
                table: "WikiGameNavigations",
                columns: new[] { "Date", "UserName", "Step" });
        }
    }
}
