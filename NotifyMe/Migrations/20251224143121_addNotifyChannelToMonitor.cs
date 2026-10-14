using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotifyMe.App.Migrations
{
    /// <inheritdoc />
    public partial class addNotifyChannelToMonitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationChannel",
                table: "Monitors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationChannel",
                table: "Monitors");
        }
    }
}
