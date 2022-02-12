using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RLStats_Classes.Migrations
{
    public partial class Iteration4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomId",
                table: "Id",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomId",
                table: "Id");
        }
    }
}
