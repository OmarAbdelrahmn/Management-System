using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeEvaluationDecisionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAt",
                table: "EmployeeEvaluations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionNotes",
                table: "EmployeeEvaluations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EmployeeEvaluations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEvaluations_Status",
                table: "EmployeeEvaluations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeEvaluations_Status",
                table: "EmployeeEvaluations");

            migrationBuilder.DropColumn(
                name: "DecidedAt",
                table: "EmployeeEvaluations");

            migrationBuilder.DropColumn(
                name: "DecisionNotes",
                table: "EmployeeEvaluations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EmployeeEvaluations");
        }
    }
}
