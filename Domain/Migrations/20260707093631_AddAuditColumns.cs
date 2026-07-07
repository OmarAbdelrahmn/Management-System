using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VoteSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "VoteSessions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VoteSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "VoteSessions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Votes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Votes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Votes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "MeetingNotes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MeetingNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "MeetingNotes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "MeetingMinutes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MeetingMinutes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "MeetingMinutes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "MeetingInvitations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MeetingInvitations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "MeetingInvitations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MeetingAgendaItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "MeetingAgendaItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MeetingAgendaItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "MeetingAgendaItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "EmailOutbox",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EmailOutbox",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "EmailOutbox",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Decisions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Decisions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Decisions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Boards",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Boards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Boards",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BoardMemberships",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "BoardMemberships",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BoardMemberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "BoardMemberships",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "BoardMeetings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BoardMeetings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "BoardMeetings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "BoardCycles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BoardCycles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "BoardCycles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AuditLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VoteSessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "VoteSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VoteSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "VoteSessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeetingNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MeetingNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MeetingNotes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeetingMinutes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MeetingMinutes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MeetingMinutes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeetingInvitations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MeetingInvitations");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MeetingInvitations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MeetingAgendaItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeetingAgendaItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MeetingAgendaItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MeetingAgendaItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "EmailOutbox");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EmailOutbox");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "EmailOutbox");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Decisions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Decisions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Decisions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BoardMemberships");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "BoardMemberships");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BoardMemberships");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "BoardMemberships");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "BoardCycles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BoardCycles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "BoardCycles");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AspNetUsers");
        }
    }
}
