using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class AddWHAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MainAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WHAdditionnalAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    MainAccountId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WHAdditionnalAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WHAdditionnalAccount_MainAccounts_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainAccounts_CharacterId",
                table: "MainAccounts",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WHAdditionnalAccount_MainAccountId",
                table: "WHAdditionnalAccount",
                column: "MainAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WHAdditionnalAccount");

            migrationBuilder.DropTable(
                name: "MainAccounts");
        }
    }
}
