using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "UserInfo");

            migrationBuilder.AddColumn<Guid>(
                name: "UserGroupId",
                table: "UserInfo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroup", x => x.Id);
                });

            var adminGroupId = Guid.NewGuid();
            migrationBuilder.InsertData(
                table: "UserGroup",
                columns: ["Id", "DisplayName", "IsAdmin"],
                values: [adminGroupId, "Admin group", true ]);

            var adminHash = SignInRepository.HashPassword("admin");
            migrationBuilder.InsertData(
                table: "UserInfo",
                columns: ["UserName", "DisplayName", "PasswordHash", "UserGroupId"],
                values: ["admin", "admin", adminHash, adminGroupId]);

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_UserGroupId",
                table: "UserInfo",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfo_UserGroup_UserGroupId",
                table: "UserInfo",
                column: "UserGroupId",
                principalTable: "UserGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfo_UserGroup_UserGroupId",
                table: "UserInfo");

            migrationBuilder.DropTable(
                name: "UserGroup");

            migrationBuilder.DropIndex(
                name: "IX_UserInfo_UserGroupId",
                table: "UserInfo");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "UserInfo");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "UserInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
