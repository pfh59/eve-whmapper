using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class Update_Route_To_Be_Unique_For_Multi_Map : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Routes_SolarSystemId_EveEntityId",
                table: "Routes");

            migrationBuilder.AddColumn<int>(
                name: "MapId",
                table: "Routes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_MapId_SolarSystemId_EveEntityId",
                table: "Routes",
                columns: new[] { "MapId", "SolarSystemId", "EveEntityId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Maps_MapId",
                table: "Routes",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Maps_MapId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_MapId_SolarSystemId_EveEntityId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "MapId",
                table: "Routes");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_SolarSystemId_EveEntityId",
                table: "Routes",
                columns: new[] { "SolarSystemId", "EveEntityId" },
                unique: true);
        }
    }
}
