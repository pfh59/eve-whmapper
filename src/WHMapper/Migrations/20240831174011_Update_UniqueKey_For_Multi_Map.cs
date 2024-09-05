using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class Update_UniqueKey_For_Multi_Map : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Systems_Name",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_Systems_SoloarSystemId",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_Systems_WHMapId",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_SystemLinks_IdWHSystemFrom_IdWHSystemTo",
                table: "SystemLinks");

            migrationBuilder.DropIndex(
                name: "IX_SystemLinks_WHMapId",
                table: "SystemLinks");

            migrationBuilder.CreateIndex(
                name: "IX_Systems_WHMapId_Name",
                table: "Systems",
                columns: new[] { "WHMapId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Systems_WHMapId_SoloarSystemId",
                table: "Systems",
                columns: new[] { "WHMapId", "SoloarSystemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLinks_IdWHSystemFrom",
                table: "SystemLinks",
                column: "IdWHSystemFrom");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLinks_WHMapId_IdWHSystemFrom_IdWHSystemTo",
                table: "SystemLinks",
                columns: new[] { "WHMapId", "IdWHSystemFrom", "IdWHSystemTo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Systems_WHMapId_Name",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_Systems_WHMapId_SoloarSystemId",
                table: "Systems");

            migrationBuilder.DropIndex(
                name: "IX_SystemLinks_IdWHSystemFrom",
                table: "SystemLinks");

            migrationBuilder.DropIndex(
                name: "IX_SystemLinks_WHMapId_IdWHSystemFrom_IdWHSystemTo",
                table: "SystemLinks");

            migrationBuilder.CreateIndex(
                name: "IX_Systems_Name",
                table: "Systems",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Systems_SoloarSystemId",
                table: "Systems",
                column: "SoloarSystemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Systems_WHMapId",
                table: "Systems",
                column: "WHMapId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLinks_IdWHSystemFrom_IdWHSystemTo",
                table: "SystemLinks",
                columns: new[] { "IdWHSystemFrom", "IdWHSystemTo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLinks_WHMapId",
                table: "SystemLinks",
                column: "WHMapId");
        }
    }
}
