using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddSecureAttachmentVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FileAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurgeAfter",
                table: "FileAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FileAssetVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileAssetId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ScanStatus = table.Column<int>(type: "int", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAssetVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileAssetVersions_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileAssetVersions_FileAssets_FileAssetId",
                        column: x => x.FileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_DeletedAt",
                table: "FileAssets",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FileAssetVersions_FileAssetId_VersionNumber",
                table: "FileAssetVersions",
                columns: new[] { "FileAssetId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileAssetVersions_UploadedByUserId",
                table: "FileAssetVersions",
                column: "UploadedByUserId");

            migrationBuilder.Sql(@"
                INSERT INTO FileAssetVersions
                    (FileAssetId, VersionNumber, FileName, ContentType, SizeBytes, StoragePath, Sha256, ScanStatus, ScannedAt, UploadedByUserId, CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId)
                SELECT
                    Id, 1, FileName, ContentType, SizeBytes, StoragePath, '', 2, NULL, UploadedByUserId, CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId
                FROM FileAssets;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileAssetVersions");

            migrationBuilder.DropIndex(
                name: "IX_FileAssets_DeletedAt",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "PurgeAfter",
                table: "FileAssets");
        }
    }
}
