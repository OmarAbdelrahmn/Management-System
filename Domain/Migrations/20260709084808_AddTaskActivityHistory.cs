using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskActivityHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagementTaskActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagementTaskId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: true),
                    FromAssigneeUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ToAssigneeUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: true),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementTaskActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementTaskActivities_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTaskActivities_AspNetUsers_FromAssigneeUserId",
                        column: x => x.FromAssigneeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTaskActivities_AspNetUsers_ToAssigneeUserId",
                        column: x => x.ToAssigneeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTaskActivities_ManagementTasks_ManagementTaskId",
                        column: x => x.ManagementTaskId,
                        principalTable: "ManagementTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskActivities_ActorUserId",
                table: "ManagementTaskActivities",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskActivities_FromAssigneeUserId",
                table: "ManagementTaskActivities",
                column: "FromAssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskActivities_ManagementTaskId_ActionAt",
                table: "ManagementTaskActivities",
                columns: new[] { "ManagementTaskId", "ActionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskActivities_ToAssigneeUserId",
                table: "ManagementTaskActivities",
                column: "ToAssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskActivities_Type_ActionAt",
                table: "ManagementTaskActivities",
                columns: new[] { "Type", "ActionAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagementTaskActivities");
        }
    }
}
