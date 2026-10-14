using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotifyMe.App.Migrations
{
    /// <inheritdoc />
    public partial class attachNotificationToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Customers_CustomerID",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "Notifications",
                newName: "ApplicationUserID");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_CustomerID",
                table: "Notifications",
                newName: "IX_Notifications_ApplicationUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_ApplicationUserID",
                table: "Notifications",
                column: "ApplicationUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_ApplicationUserID",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserID",
                table: "Notifications",
                newName: "CustomerID");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_ApplicationUserID",
                table: "Notifications",
                newName: "IX_Notifications_CustomerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Customers_CustomerID",
                table: "Notifications",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
