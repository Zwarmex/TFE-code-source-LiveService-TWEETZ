using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class init_signalr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHAT_MESSAGES",
                columns: table => new
                {
                    MSG_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LIVE_ApiVideoLiveStreamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MSG_SenderId = table.Column<int>(type: "int", nullable: false),
                    MSG_SenderUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MSG_Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MSG_SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MSG_IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_MESSAGES", x => x.MSG_Id);
                    table.ForeignKey(
                        name: "FK_CHAT_MESSAGES_LIVE_LIVE_ApiVideoLiveStreamId",
                        column: x => x.LIVE_ApiVideoLiveStreamId,
                        principalTable: "LIVE",
                        principalColumn: "LIVE_ApiVideoLiveStreamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MODERATION_LOGS",
                columns: table => new
                {
                    LOG_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LIVE_ApiVideoLiveStreamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LOG_ModeratorId = table.Column<int>(type: "int", nullable: false),
                    LOG_TargetUserId = table.Column<int>(type: "int", nullable: false),
                    LOG_ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LOG_DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    LOG_ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MODERATION_LOGS", x => x.LOG_Id);
                    table.ForeignKey(
                        name: "FK_MODERATION_LOGS_LIVE_LIVE_ApiVideoLiveStreamId",
                        column: x => x.LIVE_ApiVideoLiveStreamId,
                        principalTable: "LIVE",
                        principalColumn: "LIVE_ApiVideoLiveStreamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_MESSAGES_LIVE_ApiVideoLiveStreamId",
                table: "CHAT_MESSAGES",
                column: "LIVE_ApiVideoLiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_MODERATION_LOGS_LIVE_ApiVideoLiveStreamId",
                table: "MODERATION_LOGS",
                column: "LIVE_ApiVideoLiveStreamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHAT_MESSAGES");

            migrationBuilder.DropTable(
                name: "MODERATION_LOGS");
        }
    }
}
