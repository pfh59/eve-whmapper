using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WHMapper.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsEOLWithEOLStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsEndOfLifeConnection",
                table: "SystemLinks",
                newName: "EndOfLifeStatus");

            migrationBuilder.AlterColumn<int>(
                name: "EndOfLifeStatus",
                table: "SystemLinks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EndOfLifeStatus",
                table: "SystemLinks",
                newName: "IsEndOfLifeConnection");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEndOfLifeConnection",
                table: "SystemLinks",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
