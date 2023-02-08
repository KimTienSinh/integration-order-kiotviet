using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kps.Integration.Api.Migrations
{
    public partial class Update_ScheduleLogging_Add_ApplicationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                table: "ScheduleLogging",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationName",
                table: "ScheduleLogging");
        }
    }
}
