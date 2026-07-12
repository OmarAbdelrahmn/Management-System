using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollDecisionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "EmployeePayrollRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionNotes",
                table: "EmployeePayrollRecords",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "EmployeePayrollRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "EmployeePayrollRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollRecords_PayrollMonth_Status",
                table: "EmployeePayrollRecords",
                columns: new[] { "PayrollMonth", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeePayrollRecords_PayrollMonth_Status",
                table: "EmployeePayrollRecords");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "EmployeePayrollRecords");

            migrationBuilder.DropColumn(
                name: "DecisionNotes",
                table: "EmployeePayrollRecords");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "EmployeePayrollRecords");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "EmployeePayrollRecords");
        }
    }
}
