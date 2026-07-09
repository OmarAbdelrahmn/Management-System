using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionalExcellenceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GovernanceCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RoadmapNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MeasureType = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ReportingPeriod = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMeasures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StrategicPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Vision = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Mission = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategicPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GovernanceCycleId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TargetScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    VerificationNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FinancialIndicatorValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernanceCriteria_GovernanceCycles_GovernanceCycleId",
                        column: x => x.GovernanceCycleId,
                        principalTable: "GovernanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GovernanceCycleId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernanceTasks_GovernanceCycles_GovernanceCycleId",
                        column: x => x.GovernanceCycleId,
                        principalTable: "GovernanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StrategicPerspectives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategicPlanId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategicPerspectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategicPerspectives_StrategicPlans_StrategicPlanId",
                        column: x => x.StrategicPlanId,
                        principalTable: "StrategicPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategicVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategicPlanId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsAutomated = table.Column<bool>(type: "bit", nullable: false),
                    LastFetchedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategicVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategicVariables_StrategicPlans_StrategicPlanId",
                        column: x => x.StrategicPlanId,
                        principalTable: "StrategicPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GovernanceCriterionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernanceAttachments_GovernanceCriteria_GovernanceCriterionId",
                        column: x => x.GovernanceCriterionId,
                        principalTable: "GovernanceCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategicGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategicPerspectiveId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Vision2030Alignment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SustainabilityAlignment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategicGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategicGoals_StrategicPerspectives_StrategicPerspectiveId",
                        column: x => x.StrategicPerspectiveId,
                        principalTable: "StrategicPerspectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategicIndicators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategicPlanId = table.Column<int>(type: "int", nullable: false),
                    StrategicGoalId = table.Column<int>(type: "int", nullable: true),
                    ParentIndicatorId = table.Column<int>(type: "int", nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RelatedProjectName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RelatedProgramName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategicIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategicIndicators_StrategicGoals_StrategicGoalId",
                        column: x => x.StrategicGoalId,
                        principalTable: "StrategicGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StrategicIndicators_StrategicIndicators_ParentIndicatorId",
                        column: x => x.ParentIndicatorId,
                        principalTable: "StrategicIndicators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StrategicIndicators_StrategicPlans_StrategicPlanId",
                        column: x => x.StrategicPlanId,
                        principalTable: "StrategicPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceAttachments_GovernanceCriterionId",
                table: "GovernanceAttachments",
                column: "GovernanceCriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceAttachments_UploadedAt",
                table: "GovernanceAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceCriteria_GovernanceCycleId_Status",
                table: "GovernanceCriteria",
                columns: new[] { "GovernanceCycleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceCycles_IsActive",
                table: "GovernanceCycles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceCycles_Year_Status",
                table: "GovernanceCycles",
                columns: new[] { "Year", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceTasks_GovernanceCycleId",
                table: "GovernanceTasks",
                column: "GovernanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceTasks_Status_DueDate",
                table: "GovernanceTasks",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMeasures_Code",
                table: "PerformanceMeasures",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMeasures_MeasureType_Status",
                table: "PerformanceMeasures",
                columns: new[] { "MeasureType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategicGoals_StrategicPerspectiveId_SortOrder",
                table: "StrategicGoals",
                columns: new[] { "StrategicPerspectiveId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategicIndicators_ParentIndicatorId",
                table: "StrategicIndicators",
                column: "ParentIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategicIndicators_StrategicGoalId",
                table: "StrategicIndicators",
                column: "StrategicGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategicIndicators_StrategicPlanId_Kind_Status",
                table: "StrategicIndicators",
                columns: new[] { "StrategicPlanId", "Kind", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategicPerspectives_StrategicPlanId_SortOrder",
                table: "StrategicPerspectives",
                columns: new[] { "StrategicPlanId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategicPlans_Status_StartDate_EndDate",
                table: "StrategicPlans",
                columns: new[] { "Status", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategicVariables_StrategicPlanId_IsAutomated",
                table: "StrategicVariables",
                columns: new[] { "StrategicPlanId", "IsAutomated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GovernanceAttachments");

            migrationBuilder.DropTable(
                name: "GovernanceTasks");

            migrationBuilder.DropTable(
                name: "PerformanceMeasures");

            migrationBuilder.DropTable(
                name: "StrategicIndicators");

            migrationBuilder.DropTable(
                name: "StrategicVariables");

            migrationBuilder.DropTable(
                name: "GovernanceCriteria");

            migrationBuilder.DropTable(
                name: "StrategicGoals");

            migrationBuilder.DropTable(
                name: "GovernanceCycles");

            migrationBuilder.DropTable(
                name: "StrategicPerspectives");

            migrationBuilder.DropTable(
                name: "StrategicPlans");
        }
    }
}
