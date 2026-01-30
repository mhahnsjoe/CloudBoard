using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitializeRemainingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set RemainingHours to 0 for all "Done" items
            migrationBuilder.Sql("UPDATE \"WorkItems\" SET \"RemainingHours\" = 0 WHERE \"Status\" = 'Done' AND \"RemainingHours\" IS NULL");
            
            // Set RemainingHours to EstimatedHours for all other items
            migrationBuilder.Sql("UPDATE \"WorkItems\" SET \"RemainingHours\" = \"EstimatedHours\" WHERE \"Status\" != 'Done' AND \"RemainingHours\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
