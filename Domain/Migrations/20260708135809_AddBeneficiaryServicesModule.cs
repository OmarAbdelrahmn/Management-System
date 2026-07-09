using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaryServicesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeneficiaryAidRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryProfileId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryEntityId = table.Column<int>(type: "int", nullable: true),
                    RequestNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AidType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    SocialResearchNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryAidRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryAidRequests_BeneficiaryEntities_BeneficiaryEntityId",
                        column: x => x.BeneficiaryEntityId,
                        principalTable: "BeneficiaryEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BeneficiaryAidRequests_BeneficiaryProfiles_BeneficiaryProfileId",
                        column: x => x.BeneficiaryProfileId,
                        principalTable: "BeneficiaryProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CouponRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryProfileId = table.Column<int>(type: "int", nullable: true),
                    CouponType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponRequests_BeneficiaryProfiles_BeneficiaryProfileId",
                        column: x => x.BeneficiaryProfileId,
                        principalTable: "BeneficiaryProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EntitySupportRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryEntityId = table.Column<int>(type: "int", nullable: true),
                    RequesterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SupportType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecisionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntitySupportRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntitySupportRequests_BeneficiaryEntities_BeneficiaryEntityId",
                        column: x => x.BeneficiaryEntityId,
                        principalTable: "BeneficiaryEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipRequirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BeneficiaryPaymentOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryAidRequestId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryProfileId = table.Column<int>(type: "int", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryPaymentOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryPaymentOrders_BeneficiaryAidRequests_BeneficiaryAidRequestId",
                        column: x => x.BeneficiaryAidRequestId,
                        principalTable: "BeneficiaryAidRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BeneficiaryPaymentOrders_BeneficiaryProfiles_BeneficiaryProfileId",
                        column: x => x.BeneficiaryProfileId,
                        principalTable: "BeneficiaryProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SponsorId = table.Column<int>(type: "int", nullable: false),
                    BeneficiaryProfileId = table.Column<int>(type: "int", nullable: true),
                    SponsorshipRequirementId = table.Column<int>(type: "int", nullable: true),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipRecords_BeneficiaryProfiles_BeneficiaryProfileId",
                        column: x => x.BeneficiaryProfileId,
                        principalTable: "BeneficiaryProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SponsorshipRecords_Sponsors_SponsorId",
                        column: x => x.SponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SponsorshipRecords_SponsorshipRequirements_SponsorshipRequirementId",
                        column: x => x.SponsorshipRequirementId,
                        principalTable: "SponsorshipRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SponsorshipRecordId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipPayments_SponsorshipRecords_SponsorshipRecordId",
                        column: x => x.SponsorshipRecordId,
                        principalTable: "SponsorshipRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAidRequests_BeneficiaryEntityId",
                table: "BeneficiaryAidRequests",
                column: "BeneficiaryEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAidRequests_BeneficiaryProfileId",
                table: "BeneficiaryAidRequests",
                column: "BeneficiaryProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAidRequests_RequestNumber",
                table: "BeneficiaryAidRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryAidRequests_Status_IsExternal",
                table: "BeneficiaryAidRequests",
                columns: new[] { "Status", "IsExternal" });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryPaymentOrders_BeneficiaryAidRequestId",
                table: "BeneficiaryPaymentOrders",
                column: "BeneficiaryAidRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryPaymentOrders_BeneficiaryProfileId",
                table: "BeneficiaryPaymentOrders",
                column: "BeneficiaryProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryPaymentOrders_OrderNumber",
                table: "BeneficiaryPaymentOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryPaymentOrders_OrderType_Status",
                table: "BeneficiaryPaymentOrders",
                columns: new[] { "OrderType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CouponRequests_BeneficiaryProfileId",
                table: "CouponRequests",
                column: "BeneficiaryProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRequests_Status",
                table: "CouponRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EntitySupportRequests_BeneficiaryEntityId",
                table: "EntitySupportRequests",
                column: "BeneficiaryEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntitySupportRequests_Status_IsExternal",
                table: "EntitySupportRequests",
                columns: new[] { "Status", "IsExternal" });

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_Status",
                table: "Sponsors",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipPayments_SponsorshipRecordId_Status_DueDate",
                table: "SponsorshipPayments",
                columns: new[] { "SponsorshipRecordId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipRecords_BeneficiaryProfileId",
                table: "SponsorshipRecords",
                column: "BeneficiaryProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipRecords_SponsorId_Status",
                table: "SponsorshipRecords",
                columns: new[] { "SponsorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipRecords_SponsorshipRequirementId",
                table: "SponsorshipRecords",
                column: "SponsorshipRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipRequirements_Status",
                table: "SponsorshipRequirements",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeneficiaryPaymentOrders");

            migrationBuilder.DropTable(
                name: "CouponRequests");

            migrationBuilder.DropTable(
                name: "EntitySupportRequests");

            migrationBuilder.DropTable(
                name: "SponsorshipPayments");

            migrationBuilder.DropTable(
                name: "BeneficiaryAidRequests");

            migrationBuilder.DropTable(
                name: "SponsorshipRecords");

            migrationBuilder.DropTable(
                name: "Sponsors");

            migrationBuilder.DropTable(
                name: "SponsorshipRequirements");
        }
    }
}
