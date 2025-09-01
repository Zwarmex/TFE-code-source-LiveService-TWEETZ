using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class changesRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LIVE_ViewCount",
                table: "LIVE");

            migrationBuilder.DropColumn(
                name: "LIVE_Views",
                table: "LIVE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LIVE_ViewCount",
                table: "LIVE",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LIVE_Views",
                table: "LIVE",
                type: "int",
                nullable: true);
        }
    }
}
