using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercury.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Movies",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false),
                        Title = table.Column<string>(type: "longtext", nullable: false),
                        Genre = table.Column<string>(type: "longtext", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Movies", x => x.Id);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Movies");
        }
    }
}
