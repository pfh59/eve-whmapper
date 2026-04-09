using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EveCharacterId = table.Column<int>(type: "integer", nullable: false),
                    KeyLink = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    KeyDelete = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    KeyIncrementExtension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    KeyDecrementExtension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    KeyIncrementExtensionAlt = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    KeyDecrementExtensionAlt = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ZoomEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ZoomInverse = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMultiSelection = table.Column<bool>(type: "boolean", nullable: false),
                    LinkSnapping = table.Column<bool>(type: "boolean", nullable: false),
                    NodeSpacing = table.Column<double>(type: "double precision", nullable: false),
                    DragThreshold = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_EveCharacterId",
                table: "UserSettings",
                column: "EveCharacterId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
