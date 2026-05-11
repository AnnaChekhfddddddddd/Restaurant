using Microsoft.EntityFrameworkCore.Migrations;

namespace Restaurant.Migrations
{
    public partial class AddWaiterToPlace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "WaiterID",
                table: "Place",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Place_WaiterID",
                table: "Place",
                column: "WaiterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Place_Users_WaiterID",
                table: "Place",
                column: "WaiterID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Place_Users_WaiterID",
                table: "Place");

            migrationBuilder.DropIndex(
                name: "IX_Place_WaiterID",
                table: "Place");

            migrationBuilder.DropColumn(
                name: "WaiterID",
                table: "Place");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
