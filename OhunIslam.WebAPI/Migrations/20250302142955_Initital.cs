using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhunIslam.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumedMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StreamStatus = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StreamDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumedMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaItems",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaLecturer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateIssued = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaItems", x => x.MediaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumedMessages_StreamStartTime",
                table: "ConsumedMessages",
                column: "StreamStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumedMessages_StreamStatus",
                table: "ConsumedMessages",
                column: "StreamStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumedMessages");

            migrationBuilder.DropTable(
                name: "MediaItems");
        }
    }
}
