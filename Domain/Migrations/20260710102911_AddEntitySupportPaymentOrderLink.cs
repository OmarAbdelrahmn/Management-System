using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddEntitySupportPaymentOrderLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntitySupportRequestId",
                table: "BeneficiaryPaymentOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryPaymentOrders_EntitySupportRequestId",
                table: "BeneficiaryPaymentOrders",
                column: "EntitySupportRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_BeneficiaryPaymentOrders_EntitySupportRequests_EntitySupportRequestId",
                table: "BeneficiaryPaymentOrders",
                column: "EntitySupportRequestId",
                principalTable: "EntitySupportRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BeneficiaryPaymentOrders_EntitySupportRequests_EntitySupportRequestId",
                table: "BeneficiaryPaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_BeneficiaryPaymentOrders_EntitySupportRequestId",
                table: "BeneficiaryPaymentOrders");

            migrationBuilder.DropColumn(
                name: "EntitySupportRequestId",
                table: "BeneficiaryPaymentOrders");
        }
    }
}
