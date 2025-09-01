using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class removeFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LIVE_STATISTICS_LIVE_LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS");

            migrationBuilder.DropIndex(
                name: "IX_LIVE_STATISTICS_LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS");

            migrationBuilder.AlterColumn<string>(
                name: "LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LIVE_STATISTICS_LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                column: "LIVE_ApiVideoLiveStreamId");

            migrationBuilder.AddForeignKey(
                name: "FK_LIVE_STATISTICS_LIVE_LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                column: "LIVE_ApiVideoLiveStreamId",
                principalTable: "LIVE",
                principalColumn: "LIVE_ApiVideoLiveStreamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
