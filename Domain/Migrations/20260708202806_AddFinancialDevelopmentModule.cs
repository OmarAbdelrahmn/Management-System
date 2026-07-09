using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialDevelopmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalMarketingCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LandingPageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LeadsCount = table.Column<int>(type: "int", nullable: false),
                    DonationsCount = table.Column<int>(type: "int", nullable: false),
                    DonationsAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalMarketingCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EndowmentAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EndowmentNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    AssetType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualReturnEstimate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndowmentAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialSupporters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SupporterType = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NationalIdOrRegistrationNo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PreferredContactChannel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialSupporters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundraisingOpportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OpportunityType = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExternalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundraisingOpportunities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EndowmentContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndowmentAssetId = table.Column<int>(type: "int", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LesseeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnnualAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndowmentContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndowmentContracts_EndowmentAssets_EndowmentAssetId",
                        column: x => x.EndowmentAssetId,
                        principalTable: "EndowmentAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbandonedDonationCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundraisingOpportunityId = table.Column<int>(type: "int", nullable: true),
                    SupporterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FollowUpNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbandonedDonationCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbandonedDonationCarts_FundraisingOpportunities_FundraisingOpportunityId",
                        column: x => x.FundraisingOpportunityId,
                        principalTable: "FundraisingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DonationContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialSupporterId = table.Column<int>(type: "int", nullable: true),
                    FundraisingOpportunityId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DonationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceChannel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IsGift = table.Column<bool>(type: "bit", nullable: false),
                    GiftRecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationContributions_FinancialSupporters_FinancialSupporterId",
                        column: x => x.FinancialSupporterId,
                        principalTable: "FinancialSupporters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DonationContributions_FundraisingOpportunities_FundraisingOpportunityId",
                        column: x => x.FundraisingOpportunityId,
                        principalTable: "FundraisingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EndowmentInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndowmentAssetId = table.Column<int>(type: "int", nullable: false),
                    EndowmentContractId = table.Column<int>(type: "int", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndowmentInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndowmentInvoices_EndowmentAssets_EndowmentAssetId",
                        column: x => x.EndowmentAssetId,
                        principalTable: "EndowmentAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EndowmentInvoices_EndowmentContracts_EndowmentContractId",
                        column: x => x.EndowmentContractId,
                        principalTable: "EndowmentContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedDonationCarts_FundraisingOpportunityId",
                table: "AbandonedDonationCarts",
                column: "FundraisingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedDonationCarts_Status_CartDate",
                table: "AbandonedDonationCarts",
                columns: new[] { "Status", "CartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalMarketingCampaigns_Channel_Status",
                table: "DigitalMarketingCampaigns",
                columns: new[] { "Channel", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationContributions_FinancialSupporterId",
                table: "DonationContributions",
                column: "FinancialSupporterId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationContributions_FundraisingOpportunityId",
                table: "DonationContributions",
                column: "FundraisingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationContributions_SourceChannel",
                table: "DonationContributions",
                column: "SourceChannel");

            migrationBuilder.CreateIndex(
                name: "IX_DonationContributions_Status_DonationDate",
                table: "DonationContributions",
                columns: new[] { "Status", "DonationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EndowmentAssets_EndowmentNumber",
                table: "EndowmentAssets",
                column: "EndowmentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EndowmentAssets_Status_AssetType",
                table: "EndowmentAssets",
                columns: new[] { "Status", "AssetType" });

            migrationBuilder.CreateIndex(
                name: "IX_EndowmentContracts_EndowmentAssetId",
                table: "EndowmentContracts",
                column: "EndowmentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_EndowmentContracts_Status_EndDate",
                table: "EndowmentContracts",
                columns: new[] { "Status", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EndowmentInvoices_EndowmentAssetId",
                table: "EndowmentInvoices",
                column: "EndowmentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_EndowmentInvoices_EndowmentContractId",
                table: "EndowmentInvoices",
                column: "EndowmentContractId");

            migrationBuilder.CreateIndex(
                name: "IX_EndowmentInvoices_Status_DueDate",
                table: "EndowmentInvoices",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialSupporters_Mobile",
                table: "FinancialSupporters",
                column: "Mobile");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialSupporters_Status_SupporterType",
                table: "FinancialSupporters",
                columns: new[] { "Status", "SupporterType" });

            migrationBuilder.CreateIndex(
                name: "IX_FundraisingOpportunities_OpportunityType_Status",
                table: "FundraisingOpportunities",
                columns: new[] { "OpportunityType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FundraisingOpportunities_ReferenceNumber",
                table: "FundraisingOpportunities",
                column: "ReferenceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbandonedDonationCarts");

            migrationBuilder.DropTable(
                name: "DigitalMarketingCampaigns");

            migrationBuilder.DropTable(
                name: "DonationContributions");

            migrationBuilder.DropTable(
                name: "EndowmentInvoices");

            migrationBuilder.DropTable(
                name: "FinancialSupporters");

            migrationBuilder.DropTable(
                name: "FundraisingOpportunities");

            migrationBuilder.DropTable(
                name: "EndowmentContracts");

            migrationBuilder.DropTable(
                name: "EndowmentAssets");
        }
    }
}
