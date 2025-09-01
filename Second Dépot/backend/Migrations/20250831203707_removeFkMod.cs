using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class removeFkMod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MODERATION_LOGS_LIVE_LIVE_ApiVideoLiveStreamId",
                table: "MODERATION_LOGS");

            migrationBuilder.DropIndex(
                name: "IX_MODERATION_LOGS_LIVE_ApiVideoLiveStreamId",
                table: "MODERATION_LOGS");

            migrationBuilder.AlterColumn<string>(
                name: "LIVE_ApiVideoLiveStreamId",
                table: "MODERATION_LOGS",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LIVE_ApiVideoLiveStreamId",
                table: "MODERATION_LOGS",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_MODERATION_LOGS_LIVE_ApiVideoLiveStreamId",
                table: "MODERATION_LOGS",
                column: "LIVE_ApiVideoLiveStreamId");

            migrationBuilder.AddForeignKey(
                name: "FK_MODERATION_LOGS_LIVE_LIVE_ApiVideoLiveStreamId",
                table: "MODERATION_LOGS",
                column: "LIVE_ApiVideoLiveStreamId",
                principalTable: "LIVE",
                principalColumn: "LIVE_ApiVideoLiveStreamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
