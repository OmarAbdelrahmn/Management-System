using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalDelegationAndDeadlines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultDeadlineHours",
                table: "ApprovalRoutes",
                type: "int",
                nullable: false,
                defaultValue: 72);

            migrationBuilder.AddColumn<string>(
                name: "CurrentApproverUserId",
                table: "ApprovalRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueAt",
                table: "ApprovalRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalationCount",
                table: "ApprovalRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEscalatedAt",
                table: "ApprovalRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelegatedToUserId",
                table: "ApprovalActions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE request
                SET CurrentApproverUserId = step.ApproverUserId,
                    DueAt = CASE WHEN request.Status = 0 THEN DATEADD(HOUR, route.DefaultDeadlineHours, request.CreatedAt) ELSE NULL END
                FROM ApprovalRequests AS request
                INNER JOIN ApprovalRoutes AS route ON route.Id = request.ApprovalRouteId
                INNER JOIN ApprovalSteps AS step ON step.ApprovalRouteId = request.ApprovalRouteId AND step.StepOrder = request.CurrentStepOrder;

                IF EXISTS (SELECT 1 FROM ApprovalRequests WHERE CurrentApproverUserId IS NULL)
                    THROW 51000, 'Approval request migration failed: every existing request must have a matching route step.', 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentApproverUserId",
                table: "ApprovalRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_CurrentApproverUserId",
                table: "ApprovalRequests",
                column: "CurrentApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_Status_DueAt_EscalationCount",
                table: "ApprovalRequests",
                columns: new[] { "Status", "DueAt", "EscalationCount" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalActions_DelegatedToUserId",
                table: "ApprovalActions",
                column: "DelegatedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalActions_AspNetUsers_DelegatedToUserId",
                table: "ApprovalActions",
                column: "DelegatedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequests_AspNetUsers_CurrentApproverUserId",
                table: "ApprovalRequests",
                column: "CurrentApproverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalActions_AspNetUsers_DelegatedToUserId",
                table: "ApprovalActions");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequests_AspNetUsers_CurrentApproverUserId",
                table: "ApprovalRequests");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequests_CurrentApproverUserId",
                table: "ApprovalRequests");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequests_Status_DueAt_EscalationCount",
                table: "ApprovalRequests");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalActions_DelegatedToUserId",
                table: "ApprovalActions");

            migrationBuilder.DropColumn(
                name: "DefaultDeadlineHours",
                table: "ApprovalRoutes");

            migrationBuilder.DropColumn(
                name: "CurrentApproverUserId",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "DueAt",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "EscalationCount",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "LastEscalatedAt",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "DelegatedToUserId",
                table: "ApprovalActions");
        }
    }
}
