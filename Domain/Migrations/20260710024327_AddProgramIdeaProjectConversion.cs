using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramIdeaProjectConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConvertedProjectId",
                table: "ProgramIdeas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramIdeas_ConvertedProjectId",
                table: "ProgramIdeas",
                column: "ConvertedProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramIdeas_ProgramProjects_ConvertedProjectId",
                table: "ProgramIdeas",
                column: "ConvertedProjectId",
                principalTable: "ProgramProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgramIdeas_ProgramProjects_ConvertedProjectId",
                table: "ProgramIdeas");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIdeas_ConvertedProjectId",
                table: "ProgramIdeas");

            migrationBuilder.DropColumn(
                name: "ConvertedProjectId",
                table: "ProgramIdeas");
        }
    }
}
