using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kps.Integration.Api.Migrations
{
    public partial class Update_WmsSyncLogTable_Add_Message_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "WmsSyncLog",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "WmsSyncLog");
        }
    }
}
