using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RLStats_Classes.Migrations
{
    public partial class Iteration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdvancedPlayer",
                table: "AdvancedPlayer");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdvancedPlayer",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(95)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomId",
                table: "AdvancedPlayer",
                type: "varchar(95)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdvancedPlayer",
                table: "AdvancedPlayer",
                column: "CustomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdvancedPlayer",
                table: "AdvancedPlayer");

            migrationBuilder.DropColumn(
                name: "CustomId",
                table: "AdvancedPlayer");

            migrationBuilder.UpdateData(
                table: "AdvancedPlayer",
                keyColumn: "Name",
                keyValue: null,
                column: "Name",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdvancedPlayer",
                type: "varchar(95)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdvancedPlayer",
                table: "AdvancedPlayer",
                column: "Name");
        }
    }
}
