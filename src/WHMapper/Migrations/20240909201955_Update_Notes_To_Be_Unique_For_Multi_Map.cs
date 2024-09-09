using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class Update_Notes_To_Be_Unique_For_Multi_Map : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_SoloarSystemId",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "MapId",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_MapId_SoloarSystemId",
                table: "Notes",
                columns: new[] { "MapId", "SoloarSystemId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Maps_MapId",
                table: "Notes",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Maps_MapId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_MapId_SoloarSystemId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "MapId",
                table: "Notes");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_SoloarSystemId",
                table: "Notes",
                column: "SoloarSystemId",
                unique: true);
        }
    }
}
