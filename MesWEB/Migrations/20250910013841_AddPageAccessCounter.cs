using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MesWEB.Migrations
{
    /// <inheritdoc />
    public partial class AddPageAccessCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageAccessCounters",
                columns: table => new
                {
                    PageName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageAccessCounters", x => x.PageName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageAccessCounters");
        }
    }
}
