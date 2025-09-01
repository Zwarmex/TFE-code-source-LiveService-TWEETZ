using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class addingStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LIVE_STATISTICS",
                columns: table => new
                {
                    STAT_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LIVE_ApiVideoLiveStreamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    STAT_UniqueViewers = table.Column<int>(type: "int", nullable: false),
                    STAT_TotalMessages = table.Column<int>(type: "int", nullable: false),
                    STAT_TotalDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    STAT_AvgWatchDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    STAT_MessagesPerUser = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    STAT_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    STAT_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LIVE_STATISTICS", x => x.STAT_Id);
                    table.ForeignKey(
                        name: "FK_LIVE_STATISTICS_LIVE_LIVE_ApiVideoLiveStreamId",
                        column: x => x.LIVE_ApiVideoLiveStreamId,
                        principalTable: "LIVE",
                        principalColumn: "LIVE_ApiVideoLiveStreamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LIVE_VIEWERS",
                columns: table => new
                {
                    LIVI_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LIVI_ApiVideoLiveStreamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LIVI_UserId = table.Column<int>(type: "int", nullable: false),
                    LIVI_JoinAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LIVI_LeaveAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LIVE_VIEWERS", x => x.LIVI_Id);
                    table.ForeignKey(
                        name: "FK_LIVE_VIEWERS_LIVE_LIVI_ApiVideoLiveStreamId",
                        column: x => x.LIVI_ApiVideoLiveStreamId,
                        principalTable: "LIVE",
                        principalColumn: "LIVE_ApiVideoLiveStreamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LIVE_STATISTICS_LIVE_ApiVideoLiveStreamId",
                table: "LIVE_STATISTICS",
                column: "LIVE_ApiVideoLiveStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LIVE_VIEWERS_LIVI_ApiVideoLiveStreamId",
                table: "LIVE_VIEWERS",
                column: "LIVI_ApiVideoLiveStreamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LIVE_STATISTICS");

            migrationBuilder.DropTable(
                name: "LIVE_VIEWERS");
        }
    }
}
