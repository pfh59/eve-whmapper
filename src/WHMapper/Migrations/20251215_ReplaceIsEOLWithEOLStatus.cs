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
            // PostgreSQL cannot automatically cast boolean to integer,
            // so we use raw SQL with explicit USING clause.
            // false (not EOL) -> 0 (Normal)
            // true (EOL) -> 1 (EOL4h)
            migrationBuilder.Sql(
                @"ALTER TABLE ""SystemLinks"" 
                  RENAME COLUMN ""IsEndOfLifeConnection"" TO ""EndOfLifeStatus"";
                  
                  ALTER TABLE ""SystemLinks"" 
                  ALTER COLUMN ""EndOfLifeStatus"" TYPE integer 
                  USING CASE WHEN ""EndOfLifeStatus""::boolean THEN 1 ELSE 0 END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert back: 0 (Normal) -> false, anything else -> true
            migrationBuilder.Sql(
                @"ALTER TABLE ""SystemLinks"" 
                  ALTER COLUMN ""EndOfLifeStatus"" TYPE boolean 
                  USING (""EndOfLifeStatus"" <> 0);
                  
                  ALTER TABLE ""SystemLinks"" 
                  RENAME COLUMN ""EndOfLifeStatus"" TO ""IsEndOfLifeConnection"";");
        }
    }
}
