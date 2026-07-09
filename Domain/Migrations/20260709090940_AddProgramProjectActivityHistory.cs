using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramProjectActivityHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramProjectActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramProjectId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FromStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramProjectActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramProjectActivities_ProgramProjects_ProgramProjectId",
                        column: x => x.ProgramProjectId,
                        principalTable: "ProgramProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProjectActivities_ProgramProjectId_OccurredAt",
                table: "ProgramProjectActivities",
                columns: new[] { "ProgramProjectId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProjectActivities_Type",
                table: "ProgramProjectActivities",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramProjectActivities");
        }
    }
}
