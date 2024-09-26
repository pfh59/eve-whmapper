using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EveEntityId = table.Column<int>(type: "integer", nullable: false),
                    EveEntityName = table.Column<string>(type: "text", nullable: false),
                    EveEntity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EveCharacterId = table.Column<int>(type: "integer", nullable: false),
                    EveCharacterName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Maps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Systems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WHMapId = table.Column<int>(type: "integer", nullable: false),
                    SoloarSystemId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameExtension = table.Column<byte>(type: "smallint", nullable: false),
                    SecurityStatus = table.Column<float>(type: "real", nullable: false),
                    PosX = table.Column<double>(type: "double precision", nullable: false),
                    PosY = table.Column<double>(type: "double precision", nullable: false),
                    Locked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Systems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Systems_Maps_WHMapId",
                        column: x => x.WHMapId,
                        principalTable: "Maps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Signatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WHId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Group = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signatures_Systems_WHId",
                        column: x => x.WHId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WHMapId = table.Column<int>(type: "integer", nullable: false),
                    IdWHSystemFrom = table.Column<int>(type: "integer", nullable: false),
                    IdWHSystemTo = table.Column<int>(type: "integer", nullable: false),
                    IsEndOfLifeConnection = table.Column<bool>(type: "boolean", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    MassStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemLinks_Maps_WHMapId",
                        column: x => x.WHMapId,
                        principalTable: "Maps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemLinks_Systems_IdWHSystemFrom",
                        column: x => x.IdWHSystemFrom,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemLinks_Systems_IdWHSystemTo",
                        column: x => x.IdWHSystemTo,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_EveEntityId_EveEntity",
                table: "Accesses",
                columns: new[] { "EveEntityId", "EveEntity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_EveCharacterId",
                table: "Admins",
                column: "EveCharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maps_Name",
                table: "Maps",
                column: "Name",
                unique: true);

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
                name: "IX_SystemLinks_IdWHSystemFrom_IdWHSystemTo",
                table: "SystemLinks",
                columns: new[] { "IdWHSystemFrom", "IdWHSystemTo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLinks_IdWHSystemTo",
                table: "SystemLinks",
                column: "IdWHSystemTo");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLinks_WHMapId",
                table: "SystemLinks",
                column: "WHMapId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accesses");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Signatures");

            migrationBuilder.DropTable(
                name: "SystemLinks");

            migrationBuilder.DropTable(
                name: "Systems");

            migrationBuilder.DropTable(
                name: "Maps");
        }
    }
}
