using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentEmployeeConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConvertedEmployeeProfileId",
                table: "RecruitmentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequests_ConvertedEmployeeProfileId",
                table: "RecruitmentRequests",
                column: "ConvertedEmployeeProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecruitmentRequests_EmployeeProfiles_ConvertedEmployeeProfileId",
                table: "RecruitmentRequests",
                column: "ConvertedEmployeeProfileId",
                principalTable: "EmployeeProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecruitmentRequests_EmployeeProfiles_ConvertedEmployeeProfileId",
                table: "RecruitmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_RecruitmentRequests_ConvertedEmployeeProfileId",
                table: "RecruitmentRequests");

            migrationBuilder.DropColumn(
                name: "ConvertedEmployeeProfileId",
                table: "RecruitmentRequests");
        }
    }
}
