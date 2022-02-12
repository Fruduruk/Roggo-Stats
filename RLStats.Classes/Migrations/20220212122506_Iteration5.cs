using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RLStats_Classes.Migrations
{
    public partial class Iteration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedPlayer_Id_ID",
                table: "AdvancedPlayer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Id",
                table: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "AdvancedPlayer",
                newName: "IdCustomId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvancedPlayer_ID",
                table: "AdvancedPlayer",
                newName: "IX_AdvancedPlayer_IdCustomId");

            migrationBuilder.UpdateData(
                table: "Id",
                keyColumn: "CustomId",
                keyValue: null,
                column: "CustomId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CustomId",
                table: "Id",
                type: "varchar(95)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ID",
                table: "Id",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(95)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Id",
                table: "Id",
                column: "CustomId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedPlayer_Id_IdCustomId",
                table: "AdvancedPlayer",
                column: "IdCustomId",
                principalTable: "Id",
                principalColumn: "CustomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedPlayer_Id_IdCustomId",
                table: "AdvancedPlayer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Id",
                table: "Id");

            migrationBuilder.RenameColumn(
                name: "IdCustomId",
                table: "AdvancedPlayer",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_AdvancedPlayer_IdCustomId",
                table: "AdvancedPlayer",
                newName: "IX_AdvancedPlayer_ID");

            migrationBuilder.UpdateData(
                table: "Id",
                keyColumn: "ID",
                keyValue: null,
                column: "ID",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ID",
                table: "Id",
                type: "varchar(95)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CustomId",
                table: "Id",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(95)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Id",
                table: "Id",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedPlayer_Id_ID",
                table: "AdvancedPlayer",
                column: "ID",
                principalTable: "Id",
                principalColumn: "ID");
        }
    }
}
