using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationFollowUpModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowUpCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RequestedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionNote = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    CompletionSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNote = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FollowUpActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowUpCaseId = table.Column<int>(type: "int", nullable: true),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequiresNextAction = table.Column<bool>(type: "bit", nullable: false),
                    NextActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowUpActivities_FollowUpCases_FollowUpCaseId",
                        column: x => x.FollowUpCaseId,
                        principalTable: "FollowUpCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpActivities_FollowUpCaseId",
                table: "FollowUpActivities",
                column: "FollowUpCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpActivities_RequiresNextAction",
                table: "FollowUpActivities",
                column: "RequiresNextAction");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpActivities_SubjectType_ActivityDate",
                table: "FollowUpActivities",
                columns: new[] { "SubjectType", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpCases_CaseNumber",
                table: "FollowUpCases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpCases_Status_RequestDate",
                table: "FollowUpCases",
                columns: new[] { "Status", "RequestDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpCases_SubjectType_Status",
                table: "FollowUpCases",
                columns: new[] { "SubjectType", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowUpActivities");

            migrationBuilder.DropTable(
                name: "FollowUpCases");
        }
    }
}
