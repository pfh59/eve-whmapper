using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class Allow_Multi_Map : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WHAccessWHMap",
                columns: table => new
                {
                    WHAccessesId = table.Column<int>(type: "integer", nullable: false),
                    WHMapId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WHAccessWHMap", x => new { x.WHAccessesId, x.WHMapId });
                    table.ForeignKey(
                        name: "FK_WHAccessWHMap_Accesses_WHAccessesId",
                        column: x => x.WHAccessesId,
                        principalTable: "Accesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WHAccessWHMap_Maps_WHMapId",
                        column: x => x.WHMapId,
                        principalTable: "Maps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WHAccessWHMap_WHMapId",
                table: "WHAccessWHMap",
                column: "WHMapId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WHAccessWHMap");
        }
    }
}
