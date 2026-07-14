using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations;

[DbContext(typeof(ApplicationDbcontext))]
[Migration("20260713140000_AddSystemReportExportArchive")]
public partial class AddSystemReportExportArchive : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(name: "ArchivedContent", table: "SystemReportRuns", type: "varbinary(max)", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "ArchivedAt", table: "SystemReportRuns", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ArchiveContentType", table: "SystemReportRuns", type: "nvarchar(160)", maxLength: 160, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ArchiveFileName", table: "SystemReportRuns", type: "nvarchar(300)", maxLength: 300, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ArchivedContent", table: "SystemReportRuns");
        migrationBuilder.DropColumn(name: "ArchivedAt", table: "SystemReportRuns");
        migrationBuilder.DropColumn(name: "ArchiveContentType", table: "SystemReportRuns");
        migrationBuilder.DropColumn(name: "ArchiveFileName", table: "SystemReportRuns");
    }
}
