using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mercury.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the Rol table first
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    RolID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RollName = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.RolID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            // Now create the Users table with a foreign key reference to the Rol table
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(type: "longtext", nullable: true),
                    RolID = table.Column<int>(type: "int", nullable: false)  // Foreign key column
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);

                    // Define foreign key constraint for RolID
                    table.ForeignKey(
                        name: "FK_Users_Rol_RolID",
                        column: x => x.RolID,
                        principalTable: "Rol",
                        principalColumn: "RolID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            // Create an index on RolID for performance (optional but recommended)
            migrationBuilder.CreateIndex(
                name: "IX_Users_RolID",
                table: "Users",
                column: "RolID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the Users table first because of the foreign key constraint
            migrationBuilder.DropTable(
                name: "Users");

            // Drop the Rol table
            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
