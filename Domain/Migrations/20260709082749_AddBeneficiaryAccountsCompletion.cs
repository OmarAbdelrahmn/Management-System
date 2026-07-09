using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaryAccountsCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "BeneficiaryGuardians",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BeneficiaryGuardians",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BeneficiaryGuardians",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BeneficiaryAccountArtifacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BeneficiaryProfileId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryDependentId = table.Column<int>(type: "int", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    HolderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryAccountArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryAccountArtifacts_BeneficiaryDependents_BeneficiaryDependentId",
                        column: x => x.BeneficiaryDependentId,
                        principalTable: "BeneficiaryDependents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiaryAccountArtifacts_BeneficiaryProfiles_BeneficiaryProfileId",
                        column: x => x.BeneficiaryProfileId,
                        principalTable: "BeneficiaryProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BeneficiaryGuardianOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BeneficiaryProfileId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryGuardianId = table.Column<int>(type: "int", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryGuardianOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryGuardianOperations_BeneficiaryGuardians_BeneficiaryGuardianId",
                        column: x => x.BeneficiaryGuardianId,
                        principalTable: "BeneficiaryGuardians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiaryGuardianOperations_BeneficiaryProfiles_BeneficiaryProfileId",
                        column: x => x.BeneficiaryProfileId,
                        principalTable: "BeneficiaryProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BeneficiaryUpdateBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalProfiles = table.Column<int>(type: "int", nullable: false),
                    CompletedProfiles = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryUpdateBatches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryGuardians_BeneficiaryProfileId_IsDeleted",
                table: "BeneficiaryGuardians",
                columns: new[] { "BeneficiaryProfileId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAccountArtifacts_BeneficiaryDependentId",
                table: "BeneficiaryAccountArtifacts",
                column: "BeneficiaryDependentId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAccountArtifacts_BeneficiaryProfileId",
                table: "BeneficiaryAccountArtifacts",
                column: "BeneficiaryProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAccountArtifacts_ReferenceNumber",
                table: "BeneficiaryAccountArtifacts",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAccountArtifacts_Type_Status",
                table: "BeneficiaryAccountArtifacts",
                columns: new[] { "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryGuardianOperations_BeneficiaryGuardianId",
                table: "BeneficiaryGuardianOperations",
                column: "BeneficiaryGuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryGuardianOperations_BeneficiaryProfileId",
                table: "BeneficiaryGuardianOperations",
                column: "BeneficiaryProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryGuardianOperations_ReferenceNumber",
                table: "BeneficiaryGuardianOperations",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryGuardianOperations_Type_Status",
                table: "BeneficiaryGuardianOperations",
                columns: new[] { "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryUpdateBatches_BatchNumber",
                table: "BeneficiaryUpdateBatches",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryUpdateBatches_DueDate",
                table: "BeneficiaryUpdateBatches",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryUpdateBatches_Kind_Status",
                table: "BeneficiaryUpdateBatches",
                columns: new[] { "Kind", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeneficiaryAccountArtifacts");

            migrationBuilder.DropTable(
                name: "BeneficiaryGuardianOperations");

            migrationBuilder.DropTable(
                name: "BeneficiaryUpdateBatches");

            migrationBuilder.DropIndex(
                name: "IX_BeneficiaryGuardians_BeneficiaryProfileId_IsDeleted",
                table: "BeneficiaryGuardians");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "BeneficiaryGuardians");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BeneficiaryGuardians");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BeneficiaryGuardians");
        }
    }
}
