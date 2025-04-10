using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhunIslam.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class statsMigr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatsItems",
                columns: table => new
                {
                    TotalStreamsToday = table.Column<int>(type: "int", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatsItems");
        }
    }
}
