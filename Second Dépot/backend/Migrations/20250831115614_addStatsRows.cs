using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class addStatsRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "STAT_IsPublic",
                table: "LIVE_STATISTICS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "STAT_MaxViewers",
                table: "LIVE_STATISTICS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "STAT_UniqueChatters",
                table: "LIVE_STATISTICS",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "STAT_IsPublic",
                table: "LIVE_STATISTICS");

            migrationBuilder.DropColumn(
                name: "STAT_MaxViewers",
                table: "LIVE_STATISTICS");

            migrationBuilder.DropColumn(
                name: "STAT_UniqueChatters",
                table: "LIVE_STATISTICS");
        }
    }
}
