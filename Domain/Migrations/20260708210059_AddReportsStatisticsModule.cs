using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsStatisticsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemReportDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    SourceDomain = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemReportDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemReportRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemReportDefinitionId = table.Column<int>(type: "int", nullable: true),
                    ReportKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    FiltersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemReportRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemReportRuns_SystemReportDefinitions_SystemReportDefinitionId",
                        column: x => x.SystemReportDefinitionId,
                        principalTable: "SystemReportDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemReportDefinitions_Key",
                table: "SystemReportDefinitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemReportDefinitions_Kind_IsActive",
                table: "SystemReportDefinitions",
                columns: new[] { "Kind", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemReportRuns_ReportKey_GeneratedAt",
                table: "SystemReportRuns",
                columns: new[] { "ReportKey", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemReportRuns_SystemReportDefinitionId",
                table: "SystemReportRuns",
                column: "SystemReportDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemReportRuns");

            migrationBuilder.DropTable(
                name: "SystemReportDefinitions");
        }
    }
}
