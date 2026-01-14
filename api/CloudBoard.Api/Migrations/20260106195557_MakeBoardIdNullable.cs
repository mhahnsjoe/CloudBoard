using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeBoardIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Boards_BoardId",
                table: "WorkItems");

            migrationBuilder.AlterColumn<int>(
                name: "BoardId",
                table: "WorkItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "WorkItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId",
                table: "WorkItems",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Boards_BoardId",
                table: "WorkItems",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Boards_BoardId",
                table: "WorkItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "WorkItems");

            migrationBuilder.AlterColumn<int>(
                name: "BoardId",
                table: "WorkItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Boards_BoardId",
                table: "WorkItems",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
