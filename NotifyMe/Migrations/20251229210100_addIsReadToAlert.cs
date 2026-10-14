using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotifyMe.App.Migrations
{
    /// <inheritdoc />
    public partial class addIsReadToAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLogin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Alerts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLogin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Alerts");
        }
    }
}
