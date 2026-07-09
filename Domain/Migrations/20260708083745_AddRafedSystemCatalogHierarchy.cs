using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddRafedSystemCatalogHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalHref",
                table: "SystemPages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalIcon",
                table: "SystemPages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SystemPageGroupId",
                table: "SystemPages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconCss",
                table: "SystemModules",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemPageGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemModuleId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPageGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemPageGroups_SystemModules_SystemModuleId",
                        column: x => x.SystemModuleId,
                        principalTable: "SystemModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemPages_SystemPageGroupId",
                table: "SystemPages",
                column: "SystemPageGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemPageGroups_Key",
                table: "SystemPageGroups",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemPageGroups_SystemModuleId",
                table: "SystemPageGroups",
                column: "SystemModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemPages_SystemPageGroups_SystemPageGroupId",
                table: "SystemPages",
                column: "SystemPageGroupId",
                principalTable: "SystemPageGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SystemPages_SystemPageGroups_SystemPageGroupId",
                table: "SystemPages");

            migrationBuilder.DropTable(
                name: "SystemPageGroups");

            migrationBuilder.DropIndex(
                name: "IX_SystemPages_SystemPageGroupId",
                table: "SystemPages");

            migrationBuilder.DropColumn(
                name: "OriginalHref",
                table: "SystemPages");

            migrationBuilder.DropColumn(
                name: "OriginalIcon",
                table: "SystemPages");

            migrationBuilder.DropColumn(
                name: "SystemPageGroupId",
                table: "SystemPages");

            migrationBuilder.DropColumn(
                name: "IconCss",
                table: "SystemModules");
        }
    }
}
