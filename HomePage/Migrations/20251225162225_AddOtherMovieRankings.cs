using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomePage.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherMovieRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieRankning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Person = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ranking = table.Column<int>(type: "int", nullable: false),
                    MovieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieRankning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieRankning_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO MovieRankning (Id, Person, Ranking, MovieId)
                SELECT NEWID(), 'Anna', AnnaRanking, Id
                FROM Movie
                WHERE AnnaRanking > 0;

                INSERT INTO MovieRankning (Id, Person, Ranking, MovieId)
                SELECT NEWID(), 'Jens', JensRanking, Id
                FROM Movie
                WHERE JensRanking > 0;
                ");

            migrationBuilder.CreateIndex(
                name: "IX_MovieRankning_MovieId",
                table: "MovieRankning",
                column: "MovieId");

            migrationBuilder.DropColumn(
                name: "AnnaRanking",
                table: "Movie");

            migrationBuilder.DropColumn(
                name: "JensRanking",
                table: "Movie");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieRankning");

            migrationBuilder.AddColumn<int>(
                name: "AnnaRanking",
                table: "Movie",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JensRanking",
                table: "Movie",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
