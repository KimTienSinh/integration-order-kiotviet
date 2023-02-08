using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kps.Integration.Api.Migrations
{
    public partial class Update_OrderItemTable_Update_Nullable_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GetflyProductCode",
                table: "OrderItem",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "OrderItem",
                keyColumn: "GetflyProductCode",
                keyValue: null,
                column: "GetflyProductCode",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "GetflyProductCode",
                table: "OrderItem",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
