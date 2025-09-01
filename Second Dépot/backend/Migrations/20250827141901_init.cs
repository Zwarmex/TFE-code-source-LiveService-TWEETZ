using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tweetz.MicroServices.LiveService.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LIVE",
                columns: table => new
                {
                    LIVE_ApiVideoLiveStreamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LIVE_IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    LIVE_Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LIVE_Views = table.Column<int>(type: "int", nullable: false),
                    LIVE_StreamerId = table.Column<int>(type: "int", nullable: false),
                    LIVE_StreamerUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LIVE_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LIVE_StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LIVE_EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LIVE_PlayerUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LIVE_Broadcasting = table.Column<bool>(type: "bit", nullable: false),
                    LIVE_ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LIVE_ApiVideoStreamKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LIVE_ViewCount = table.Column<int>(type: "int", nullable: false),
                    LIVE_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LIVE_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LIVE", x => x.LIVE_ApiVideoLiveStreamId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LIVE");
        }
    }
}
