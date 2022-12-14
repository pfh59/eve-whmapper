using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WHMapper.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "SystemLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdWHSystemFrom = table.Column<int>(type: "integer", nullable: false),
                    IdWHSystemTo = table.Column<int>(type: "integer", nullable: false),
                    WHMapId = table.Column<int>(type: "integer", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Systems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SecurityStatus = table.Column<float>(type: "real", nullable: false),
                    WHMapId = table.Column<int>(type: "integer", nullable: true)
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
                    Name = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Group = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    WHSystemId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signatures_Systems_WHSystemId",
                        column: x => x.WHSystemId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Signatures_WHSystemId",
                table: "Signatures",
                column: "WHSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLinks_IdWHSystemFrom_IdWHSystemTo",
                table: "SystemLinks",
                columns: new[] { "IdWHSystemFrom", "IdWHSystemTo" },
                unique: true);

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
                name: "IX_Systems_WHMapId",
                table: "Systems",
                column: "WHMapId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
