using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class ExtendMeetingsForRafedWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "BoardMeetings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BoardMeetings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "BoardMeetings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasVoting",
                table: "BoardMeetings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Importance",
                table: "BoardMeetings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "BoardMeetings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "BoardMeetings",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumAttendancePercentage",
                table: "BoardMeetings",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "BoardMeetings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "BoardMeetings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderAt",
                table: "BoardMeetings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderEnabled",
                table: "BoardMeetings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RepeatMode",
                table: "BoardMeetings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepeatUntil",
                table: "BoardMeetings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Serial",
                table: "BoardMeetings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "BoardMeetings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MeetingApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardMeetingId = table.Column<int>(type: "int", nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingApprovals_BoardMeetings_BoardMeetingId",
                        column: x => x.BoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardMeetingId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingAttachments_BoardMeetings_BoardMeetingId",
                        column: x => x.BoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingCandidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardMeetingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingCandidates_BoardMeetings_BoardMeetingId",
                        column: x => x.BoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingGuests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardMeetingId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingGuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingGuests_BoardMeetings_BoardMeetingId",
                        column: x => x.BoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardMeetingId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingImages_BoardMeetings_BoardMeetingId",
                        column: x => x.BoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingManagers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardMeetingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingManagers_BoardMeetings_BoardMeetingId",
                        column: x => x.BoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingRepeatDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceBoardMeetingId = table.Column<int>(type: "int", nullable: false),
                    CreatedBoardMeetingId = table.Column<int>(type: "int", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRepeatDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingRepeatDrafts_BoardMeetings_CreatedBoardMeetingId",
                        column: x => x.CreatedBoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingRepeatDrafts_BoardMeetings_SourceBoardMeetingId",
                        column: x => x.SourceBoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingApprovals_BoardMeetingId",
                table: "MeetingApprovals",
                column: "BoardMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttachments_BoardMeetingId",
                table: "MeetingAttachments",
                column: "BoardMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingCandidates_BoardMeetingId_UserId",
                table: "MeetingCandidates",
                columns: new[] { "BoardMeetingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingGuests_BoardMeetingId",
                table: "MeetingGuests",
                column: "BoardMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingImages_BoardMeetingId",
                table: "MeetingImages",
                column: "BoardMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingManagers_BoardMeetingId_UserId",
                table: "MeetingManagers",
                columns: new[] { "BoardMeetingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRepeatDrafts_CreatedBoardMeetingId",
                table: "MeetingRepeatDrafts",
                column: "CreatedBoardMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRepeatDrafts_SourceBoardMeetingId",
                table: "MeetingRepeatDrafts",
                column: "SourceBoardMeetingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingApprovals");

            migrationBuilder.DropTable(
                name: "MeetingAttachments");

            migrationBuilder.DropTable(
                name: "MeetingCandidates");

            migrationBuilder.DropTable(
                name: "MeetingGuests");

            migrationBuilder.DropTable(
                name: "MeetingImages");

            migrationBuilder.DropTable(
                name: "MeetingManagers");

            migrationBuilder.DropTable(
                name: "MeetingRepeatDrafts");

            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "HasVoting",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "Importance",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "MinimumAttendancePercentage",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "ReminderAt",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "ReminderEnabled",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "RepeatMode",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "RepeatUntil",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "Serial",
                table: "BoardMeetings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "BoardMeetings");
        }
    }
}
