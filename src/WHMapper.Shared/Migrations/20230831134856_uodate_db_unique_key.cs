using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class uodate_db_unique_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Signatures_Name",
                table: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Signatures_WHId",
                table: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_EveEntityId_EveEntity",
                table: "Accesses");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_WHId_Name",
                table: "Signatures",
                columns: new[] { "WHId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_EveCharacterName",
                table: "Admins",
                column: "EveCharacterName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_EveEntityId",
                table: "Accesses",
                column: "EveEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_EveEntityName",
                table: "Accesses",
                column: "EveEntityName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Signatures_WHId_Name",
                table: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Admins_EveCharacterName",
                table: "Admins");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_EveEntityId",
                table: "Accesses");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_EveEntityName",
                table: "Accesses");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_Name",
                table: "Signatures",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_WHId",
                table: "Signatures",
                column: "WHId");

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_EveEntityId_EveEntity",
                table: "Accesses",
                columns: new[] { "EveEntityId", "EveEntity" },
                unique: true);
        }
    }
}
