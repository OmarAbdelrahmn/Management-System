using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRecipientDeliveryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "SystemNotificationRecipients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAttempts",
                table: "SystemNotificationRecipients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryStatus",
                table: "SystemNotificationRecipients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptedAt",
                table: "SystemNotificationRecipients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryError",
                table: "SystemNotificationRecipients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderReference",
                table: "SystemNotificationRecipients",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemNotificationRecipients_DeliveryStatus",
                table: "SystemNotificationRecipients",
                column: "DeliveryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SystemNotificationRecipients_RecipientUserId_DeliveryStatus",
                table: "SystemNotificationRecipients",
                columns: new[] { "RecipientUserId", "DeliveryStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemNotificationRecipients_DeliveryStatus",
                table: "SystemNotificationRecipients");

            migrationBuilder.DropIndex(
                name: "IX_SystemNotificationRecipients_RecipientUserId_DeliveryStatus",
                table: "SystemNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "SystemNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "DeliveryAttempts",
                table: "SystemNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "SystemNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "LastAttemptedAt",
                table: "SystemNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "LastDeliveryError",
                table: "SystemNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "ProviderReference",
                table: "SystemNotificationRecipients");
        }
    }
}
