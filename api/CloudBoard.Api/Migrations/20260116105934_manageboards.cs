using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CloudBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class manageboards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create BoardColumns table
            migrationBuilder.CreateTable(
                name: "BoardColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BoardId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardColumns_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId_Order",
                table: "BoardColumns",
                columns: new[] { "BoardId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId_Name",
                table: "BoardColumns",
                columns: new[] { "BoardId", "Name" },
                unique: true);

            // CRITICAL: Seed default columns for all existing boards
            migrationBuilder.Sql(@"
                INSERT INTO ""BoardColumns"" (""Name"", ""Order"", ""Category"", ""CreatedAt"", ""BoardId"")
                SELECT 'To Do', 0, 'Proposed', NOW(), ""Id""
                FROM ""Boards"";

                INSERT INTO ""BoardColumns"" (""Name"", ""Order"", ""Category"", ""CreatedAt"", ""BoardId"")
                SELECT 'In Progress', 1, 'InProgress', NOW(), ""Id""
                FROM ""Boards"";

                INSERT INTO ""BoardColumns"" (""Name"", ""Order"", ""Category"", ""CreatedAt"", ""BoardId"")
                SELECT 'Done', 2, 'Resolved', NOW(), ""Id""
                FROM ""Boards"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardColumns");
        }
    }
}
