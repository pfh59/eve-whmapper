using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    WHActivityTypeId = table.Column<int>(type: "integer", nullable: false),
                    ActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WHInstanceId = table.Column<int>(type: "integer", nullable: true),
                    WHMapId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_ActivityTypes_WHActivityTypeId",
                        column: x => x.WHActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ActivityTypes",
                columns: new[] { "Id", "Code", "IsActive", "Label" },
                values: new object[,]
                {
                    { 1, "SignatureCreated", true, "Signature created" },
                    { 2, "SignatureUpdated", true, "Signature updated" },
                    { 3, "SystemOpened", true, "System opened" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_WHActivityTypeId",
                table: "ActivityLogs",
                column: "WHActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_WHInstanceId_ActivityDate",
                table: "ActivityLogs",
                columns: new[] { "WHInstanceId", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_WHMapId_ActivityDate",
                table: "ActivityLogs",
                columns: new[] { "WHMapId", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_Code",
                table: "ActivityTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ActivityTypes");
        }
    }
}
