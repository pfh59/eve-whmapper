using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WHInstanceId",
                table: "Maps",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OwnerEveEntityId = table.Column<int>(type: "integer", nullable: false),
                    OwnerEveEntityName = table.Column<string>(type: "text", nullable: false),
                    OwnerType = table.Column<int>(type: "integer", nullable: false),
                    CreatorCharacterId = table.Column<int>(type: "integer", nullable: false),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstanceAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WHInstanceId = table.Column<int>(type: "integer", nullable: false),
                    EveEntityId = table.Column<int>(type: "integer", nullable: false),
                    EveEntityName = table.Column<string>(type: "text", nullable: false),
                    EveEntity = table.Column<int>(type: "integer", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstanceAccesses_Instances_WHInstanceId",
                        column: x => x.WHInstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstanceAdmins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WHInstanceId = table.Column<int>(type: "integer", nullable: false),
                    EveCharacterId = table.Column<int>(type: "integer", nullable: false),
                    EveCharacterName = table.Column<string>(type: "text", nullable: false),
                    IsOwner = table.Column<bool>(type: "boolean", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstanceAdmins_Instances_WHInstanceId",
                        column: x => x.WHInstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Maps_WHInstanceId",
                table: "Maps",
                column: "WHInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_InstanceAccesses_WHInstanceId_EveEntityId_EveEntity",
                table: "InstanceAccesses",
                columns: new[] { "WHInstanceId", "EveEntityId", "EveEntity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstanceAdmins_WHInstanceId_EveCharacterId",
                table: "InstanceAdmins",
                columns: new[] { "WHInstanceId", "EveCharacterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instances_OwnerEveEntityId_OwnerType",
                table: "Instances",
                columns: new[] { "OwnerEveEntityId", "OwnerType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Maps_Instances_WHInstanceId",
                table: "Maps",
                column: "WHInstanceId",
                principalTable: "Instances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maps_Instances_WHInstanceId",
                table: "Maps");

            migrationBuilder.DropTable(
                name: "InstanceAccesses");

            migrationBuilder.DropTable(
                name: "InstanceAdmins");

            migrationBuilder.DropTable(
                name: "Instances");

            migrationBuilder.DropIndex(
                name: "IX_Maps_WHInstanceId",
                table: "Maps");

            migrationBuilder.DropColumn(
                name: "WHInstanceId",
                table: "Maps");
        }
    }
}
