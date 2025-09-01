using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class userIdStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "STAT_UpdatedAt",
                table: "LIVE_STATISTICS");

            migrationBuilder.AddColumn<int>(
                name: "STAT_UserId",
                table: "LIVE_STATISTICS",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "STAT_UserId",
                table: "LIVE_STATISTICS");

            migrationBuilder.AddColumn<DateTime>(
                name: "STAT_UpdatedAt",
                table: "LIVE_STATISTICS",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
