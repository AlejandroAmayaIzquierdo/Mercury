using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mercury.Migrations
{
    /// <inheritdoc />
    public partial class addedGenreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Genre", table: "Movies");

            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "Movies",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder
                .CreateTable(
                    name: "Genre",
                    columns: table => new
                    {
                        Id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySQL:ValueGenerationStrategy",
                                MySQLValueGenerationStrategy.IdentityColumn
                            ),
                        Description = table.Column<string>(type: "longtext", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Genre", x => x.Id);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_GenreId",
                table: "Movies",
                column: "GenreId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Genre_GenreId",
                table: "Movies",
                column: "GenreId",
                principalTable: "Genre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Movies_Genre_GenreId", table: "Movies");

            migrationBuilder.DropTable(name: "Genre");

            migrationBuilder.DropIndex(name: "IX_Movies_GenreId", table: "Movies");

            migrationBuilder.DropColumn(name: "GenreId", table: "Movies");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Movies",
                type: "longtext",
                nullable: false
            );
        }
    }
}
