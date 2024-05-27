using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealApp.Migrations
{
    public partial class table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "notification");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "notification",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "notification");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "notification",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
