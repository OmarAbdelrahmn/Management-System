using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramSupplierProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramSupplierProposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: false),
                    ProgramSupplierId = table.Column<int>(type: "int", nullable: false),
                    ProposalNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecisionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConvertedContractId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSupplierProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSupplierProposals_ProgramProjectContracts_ConvertedContractId",
                        column: x => x.ConvertedContractId,
                        principalTable: "ProgramProjectContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ProgramSupplierProposals_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramSupplierProposals_ProgramSuppliers_ProgramSupplierId",
                        column: x => x.ProgramSupplierId,
                        principalTable: "ProgramSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSupplierProposals_ConvertedContractId",
                table: "ProgramSupplierProposals",
                column: "ConvertedContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSupplierProposals_ProgramProjectId_Status",
                table: "ProgramSupplierProposals",
                columns: new[] { "ProgramProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSupplierProposals_ProgramSupplierId_Status",
                table: "ProgramSupplierProposals",
                columns: new[] { "ProgramSupplierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSupplierProposals_ProposalNumber",
                table: "ProgramSupplierProposals",
                column: "ProposalNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramSupplierProposals");
        }
    }
}
