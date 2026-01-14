using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBacklogOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BacklogOrder",
                table: "WorkItems",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BacklogOrder",
                table: "WorkItems");
        }
    }
}
