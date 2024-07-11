using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class Add_Link_Jump_Log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Admins_EveCharacterName",
                table: "Admins");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_EveEntityId",
                table: "Accesses");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_EveEntityName",
                table: "Accesses");

            migrationBuilder.CreateTable(
                name: "JumpLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WHSystemLinkId = table.Column<int>(type: "integer", nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    JumpDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShipTypeId = table.Column<int>(type: "integer", nullable: false),
                    ShipItemId = table.Column<long>(type: "bigint", nullable: false),
                    ShipMass = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JumpLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JumpLogs_SystemLinks_WHSystemLinkId",
                        column: x => x.WHSystemLinkId,
                        principalTable: "SystemLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_EveEntityId_EveEntity",
                table: "Accesses",
                columns: new[] { "EveEntityId", "EveEntity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JumpLogs_CharacterId_JumpDate",
                table: "JumpLogs",
                columns: new[] { "CharacterId", "JumpDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JumpLogs_WHSystemLinkId",
                table: "JumpLogs",
                column: "WHSystemLinkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JumpLogs");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_EveEntityId_EveEntity",
                table: "Accesses");

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
    }
}
