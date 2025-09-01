using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class addTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "STAT_LiveTitle",
                table: "LIVE_STATISTICS",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "STAT_LiveTitle",
                table: "LIVE_STATISTICS");
        }
    }
}
