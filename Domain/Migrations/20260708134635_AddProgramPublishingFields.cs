using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramPublishingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "ProgramProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "ProgramProjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationFormJson",
                table: "ProgramProjects",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialProgramCategory",
                table: "ProgramProjects",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProjects_IsPublished",
                table: "ProgramProjects",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProgramProjects_IsPublished",
                table: "ProgramProjects");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "ProgramProjects");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "ProgramProjects");

            migrationBuilder.DropColumn(
                name: "RegistrationFormJson",
                table: "ProgramProjects");

            migrationBuilder.DropColumn(
                name: "SpecialProgramCategory",
                table: "ProgramProjects");
        }
    }
}
