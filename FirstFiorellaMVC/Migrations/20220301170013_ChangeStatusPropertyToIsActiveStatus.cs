using Microsoft.EntityFrameworkCore.Migrations;

namespace FirstFiorellaMVC.Migrations
{
    public partial class ChangeStatusPropertyToIsActiveStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "AspNetUsers",
                newName: "IsActive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AspNetUsers",
                newName: "Status");
        }
    }
}
