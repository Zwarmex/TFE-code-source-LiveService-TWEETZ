using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class changeFields1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LIVE_PlayerUrl",
                table: "LIVE");

            migrationBuilder.DropColumn(
                name: "LIVE_PrivateToken",
                table: "LIVE");

            migrationBuilder.DropColumn(
                name: "LIVE_ThumbnailUrl",
                table: "LIVE");

            migrationBuilder.AlterColumn<string>(
                name: "LIVE_ApiVideoStreamKey",
                table: "LIVE",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LIVE_ApiVideoStreamKey",
                table: "LIVE",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "LIVE_PlayerUrl",
                table: "LIVE",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LIVE_PrivateToken",
                table: "LIVE",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LIVE_ThumbnailUrl",
                table: "LIVE",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
