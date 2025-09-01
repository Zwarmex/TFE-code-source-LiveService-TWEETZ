using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class chagesFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LIVE_STATISTICS_LIVE_LiveApiVideoLiveStreamId",
                table: "LIVE_STATISTICS");

            migrationBuilder.DropIndex(
                name: "IX_LIVE_STATISTICS_LiveApiVideoLiveStreamId",
                table: "LIVE_STATISTICS");

            migrationBuilder.DropColumn(
                name: "LiveApiVideoLiveStreamId",
                table: "LIVE_STATISTICS");

            migrationBuilder.RenameColumn(
                name: "LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                newName: "STAT_ApiVideoLiveStreamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "STAT_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                newName: "LIVE_ApiVideoLiveStreamId");

            migrationBuilder.AddColumn<string>(
                name: "LiveApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LIVE_STATISTICS_LiveApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                column: "LiveApiVideoLiveStreamId");

            migrationBuilder.AddForeignKey(
                name: "FK_LIVE_STATISTICS_LIVE_LiveApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                column: "LiveApiVideoLiveStreamId",
                principalTable: "LIVE",
                principalColumn: "LIVE_ApiVideoLiveStreamId");
        }
    }
}
