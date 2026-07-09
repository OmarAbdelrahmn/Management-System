using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteeringModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VolunteerOpportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpportunityNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Seats = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProcedureNotes = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    ReportSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerOpportunities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerOpportunityTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerOpportunityId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerOpportunityTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerOpportunityTasks_VolunteerOpportunities_VolunteerOpportunityId",
                        column: x => x.VolunteerOpportunityId,
                        principalTable: "VolunteerOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerAttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerOpportunityId = table.Column<int>(type: "int", nullable: false),
                    VolunteerUserId = table.Column<int>(type: "int", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerAttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerAttendanceRecords_VolunteerOpportunities_VolunteerOpportunityId",
                        column: x => x.VolunteerOpportunityId,
                        principalTable: "VolunteerOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VolunteerAttendanceRecords_VolunteerUsers_VolunteerUserId",
                        column: x => x.VolunteerUserId,
                        principalTable: "VolunteerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OpportunityTitle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VolunteerUserId = table.Column<int>(type: "int", nullable: true),
                    VolunteerOpportunityId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerRequests_VolunteerOpportunities_VolunteerOpportunityId",
                        column: x => x.VolunteerOpportunityId,
                        principalTable: "VolunteerOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VolunteerRequests_VolunteerUsers_VolunteerUserId",
                        column: x => x.VolunteerUserId,
                        principalTable: "VolunteerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerAttendanceRecords_VolunteerOpportunityId_VolunteerUserId_AttendanceDate",
                table: "VolunteerAttendanceRecords",
                columns: new[] { "VolunteerOpportunityId", "VolunteerUserId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerAttendanceRecords_VolunteerUserId",
                table: "VolunteerAttendanceRecords",
                column: "VolunteerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerOpportunities_OpportunityNumber",
                table: "VolunteerOpportunities",
                column: "OpportunityNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerOpportunities_Status_StartDate",
                table: "VolunteerOpportunities",
                columns: new[] { "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerOpportunityTasks_VolunteerOpportunityId_Status",
                table: "VolunteerOpportunityTasks",
                columns: new[] { "VolunteerOpportunityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerRequests_RequestNumber",
                table: "VolunteerRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerRequests_Source_Status",
                table: "VolunteerRequests",
                columns: new[] { "Source", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerRequests_VolunteerOpportunityId",
                table: "VolunteerRequests",
                column: "VolunteerOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerRequests_VolunteerUserId",
                table: "VolunteerRequests",
                column: "VolunteerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerUsers_Status_FullName",
                table: "VolunteerUsers",
                columns: new[] { "Status", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerUsers_VolunteerNumber",
                table: "VolunteerUsers",
                column: "VolunteerNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerAttendanceRecords");

            migrationBuilder.DropTable(
                name: "VolunteerOpportunityTasks");

            migrationBuilder.DropTable(
                name: "VolunteerRequests");

            migrationBuilder.DropTable(
                name: "VolunteerOpportunities");

            migrationBuilder.DropTable(
                name: "VolunteerUsers");
        }
    }
}
