using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MesWEB.Migrations
{
    /// <inheritdoc />
    public partial class AddCellMappingTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CellMappingTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellMappingTemplates", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "CellMappingItems",
                columns: table => new
                {
                    MappingItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sheet1Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sheet1Cell = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Sheet2Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sheet2Cell = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellMappingItems", x => x.MappingItemId);
                    table.ForeignKey(
                        name: "FK_CellMappingItems_CellMappingTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CellMappingTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CellMappingItems_TemplateId",
                table: "CellMappingItems",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CellMappingItems");

            migrationBuilder.DropTable(
                name: "CellMappingTemplates");
        }
    }
}
