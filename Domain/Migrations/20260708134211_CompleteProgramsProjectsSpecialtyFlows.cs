using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class CompleteProgramsProjectsSpecialtyFlows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramCertificateTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramCertificateTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramCertificateTemplates_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProgramQualificationCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NeedSummary = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ManagementOpinion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallmentCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramQualificationCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramQualificationCases_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProgramRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: false),
                    ParticipantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExternalReference = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramRegistrations_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSessions_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSurveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuestionsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSurveys_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramCertificateIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: false),
                    ProgramCertificateTemplateId = table.Column<int>(type: "int", nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramCertificateIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramCertificateIssues_ProgramCertificateTemplates_ProgramCertificateTemplateId",
                        column: x => x.ProgramCertificateTemplateId,
                        principalTable: "ProgramCertificateTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProgramCertificateIssues_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramQualificationInstallments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramQualificationCaseId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramQualificationInstallments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramQualificationInstallments_ProgramQualificationCases_ProgramQualificationCaseId",
                        column: x => x.ProgramQualificationCaseId,
                        principalTable: "ProgramQualificationCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSessionAttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramSessionId = table.Column<int>(type: "int", nullable: false),
                    ParticipantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExternalReference = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSessionAttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSessionAttendanceRecords_ProgramSessions_ProgramSessionId",
                        column: x => x.ProgramSessionId,
                        principalTable: "ProgramSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSurveySubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramSurveyId = table.Column<int>(type: "int", nullable: false),
                    RespondentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AnswersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSurveySubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSurveySubmissions_ProgramSurveys_ProgramSurveyId",
                        column: x => x.ProgramSurveyId,
                        principalTable: "ProgramSurveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramCertificateIssues_CertificateNumber",
                table: "ProgramCertificateIssues",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramCertificateIssues_ProgramCertificateTemplateId",
                table: "ProgramCertificateIssues",
                column: "ProgramCertificateTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramCertificateIssues_ProgramProjectId_Status",
                table: "ProgramCertificateIssues",
                columns: new[] { "ProgramProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramCertificateTemplates_ProgramProjectId_IsActive",
                table: "ProgramCertificateTemplates",
                columns: new[] { "ProgramProjectId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramQualificationCases_ProgramProjectId",
                table: "ProgramQualificationCases",
                column: "ProgramProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramQualificationCases_Status",
                table: "ProgramQualificationCases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramQualificationInstallments_ProgramQualificationCaseId_Status_DueDate",
                table: "ProgramQualificationInstallments",
                columns: new[] { "ProgramQualificationCaseId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramRegistrations_ProgramProjectId_Status",
                table: "ProgramRegistrations",
                columns: new[] { "ProgramProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSessionAttendanceRecords_ProgramSessionId_Status",
                table: "ProgramSessionAttendanceRecords",
                columns: new[] { "ProgramSessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSessions_ProgramProjectId_StartsAt",
                table: "ProgramSessions",
                columns: new[] { "ProgramProjectId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSurveys_ProgramProjectId_Status",
                table: "ProgramSurveys",
                columns: new[] { "ProgramProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSurveySubmissions_ProgramSurveyId",
                table: "ProgramSurveySubmissions",
                column: "ProgramSurveyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramCertificateIssues");

            migrationBuilder.DropTable(
                name: "ProgramQualificationInstallments");

            migrationBuilder.DropTable(
                name: "ProgramRegistrations");

            migrationBuilder.DropTable(
                name: "ProgramSessionAttendanceRecords");

            migrationBuilder.DropTable(
                name: "ProgramSurveySubmissions");

            migrationBuilder.DropTable(
                name: "ProgramCertificateTemplates");

            migrationBuilder.DropTable(
                name: "ProgramQualificationCases");

            migrationBuilder.DropTable(
                name: "ProgramSessions");

            migrationBuilder.DropTable(
                name: "ProgramSurveys");
        }
    }
}
