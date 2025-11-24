using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MesWEB.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddFormulaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sheet1Formula",
                table: "CellMappingItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sheet2Formula",
                table: "CellMappingItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sheet1Formula",
                table: "CellMappingItems");

            migrationBuilder.DropColumn(
                name: "Sheet2Formula",
                table: "CellMappingItems");
        }
    }
}
