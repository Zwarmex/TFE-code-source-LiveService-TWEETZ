using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class inviteUserRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailUrl",
                table: "LIVE",
                newName: "LIVE_ThumbnailUrl");

            migrationBuilder.AddColumn<int>(
                name: "LIVE_InvitedUserId",
                table: "LIVE",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LIVE_InvitedUserId",
                table: "LIVE");

            migrationBuilder.RenameColumn(
                name: "LIVE_ThumbnailUrl",
                table: "LIVE",
                newName: "ThumbnailUrl");
        }
    }
}
