using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhunIslam.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitailCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaItems");
        }
    }
}
