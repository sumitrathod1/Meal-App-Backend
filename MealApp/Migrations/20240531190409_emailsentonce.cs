using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealApp.Migrations
{
    public partial class emailsentonce : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RenewalMailSentDate",
                table: "users",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RenewalMailSentDate",
                table: "users");
        }
    }
}
