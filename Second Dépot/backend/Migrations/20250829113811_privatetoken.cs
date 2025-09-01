using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class privatetoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LIVE_Views",
                table: "LIVE",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "LIVE_PrivateToken",
                table: "LIVE",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LIVE_PrivateToken",
                table: "LIVE");

            migrationBuilder.AlterColumn<int>(
                name: "LIVE_Views",
                table: "LIVE",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
