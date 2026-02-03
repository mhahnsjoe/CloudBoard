using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSprintCapacityAndRetrospective : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CapacityHours",
                table: "Sprints",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Retrospective",
                table: "Sprints",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetrospectiveDate",
                table: "Sprints",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacityHours",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "Retrospective",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "RetrospectiveDate",
                table: "Sprints");
        }
    }
}
