using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MesWEB.Migrations
{
    /// <inheritdoc />
    public partial class AddCellMappingLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CellMappingLabels",
                columns: table => new
                {
                    LabelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabelName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellMappingLabels", x => x.LabelId);
                });

            migrationBuilder.AddColumn<int>(
                name: "LabelId",
                table: "CellMappingTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CellMappingTemplates_LabelId",
                table: "CellMappingTemplates",
                column: "LabelId");

            migrationBuilder.AddForeignKey(
                name: "FK_CellMappingTemplates_CellMappingLabels_LabelId",
                table: "CellMappingTemplates",
                column: "LabelId",
                principalTable: "CellMappingLabels",
                principalColumn: "LabelId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CellMappingTemplates_CellMappingLabels_LabelId",
                table: "CellMappingTemplates");

            migrationBuilder.DropTable(
                name: "CellMappingLabels");

            migrationBuilder.DropIndex(
                name: "IX_CellMappingTemplates_LabelId",
                table: "CellMappingTemplates");

            migrationBuilder.DropColumn(
                name: "LabelId",
                table: "CellMappingTemplates");
        }
    }
}
