using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhunIslam.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class statsmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConsumedMessages_StreamStartTime",
                table: "ConsumedMessages");

            migrationBuilder.DropIndex(
                name: "IX_ConsumedMessages_StreamStatus",
                table: "ConsumedMessages");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "StatsItems",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "MessageContent",
                table: "StatsItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "StatsItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "MediaTitle",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MediaPath",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MediaLecturer",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MediaDescription",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "StreamStatus",
                table: "ConsumedMessages",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "ConsumedMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MediaTitle",
                table: "ConsumedMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatsItems",
                table: "StatsItems",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "StreamStatsUpdate",
                columns: table => new
                {
                    TotalStreamsToday = table.Column<int>(type: "int", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumedMessages_Id",
                table: "ConsumedMessages",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreamStatsUpdate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatsItems",
                table: "StatsItems");

            migrationBuilder.DropIndex(
                name: "IX_ConsumedMessages_Id",
                table: "ConsumedMessages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "StatsItems");

            migrationBuilder.DropColumn(
                name: "MessageContent",
                table: "StatsItems");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "StatsItems");

            migrationBuilder.AlterColumn<string>(
                name: "MediaTitle",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediaPath",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediaLecturer",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediaDescription",
                table: "MediaItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StreamStatus",
                table: "ConsumedMessages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "ConsumedMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediaTitle",
                table: "ConsumedMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsumedMessages_StreamStartTime",
                table: "ConsumedMessages",
                column: "StreamStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumedMessages_StreamStatus",
                table: "ConsumedMessages",
                column: "StreamStatus");
        }
    }
}
