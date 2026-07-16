using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;

namespace Domain;

public class ApplicationDbcontext(
    DbContextOptions<ApplicationDbcontext> options,
    ICurrentUserContext? currentUserContext = null)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardCycle> BoardCycles => Set<BoardCycle>();
    public DbSet<BoardMembership> BoardMemberships => Set<BoardMembership>();
    public DbSet<BoardMeeting> BoardMeetings => Set<BoardMeeting>();
    public DbSet<MeetingAgendaItem> MeetingAgendaItems => Set<MeetingAgendaItem>();
    public DbSet<MeetingInvitation> MeetingInvitations => Set<MeetingInvitation>();
    public DbSet<MeetingNote> MeetingNotes => Set<MeetingNote>();
    public DbSet<MeetingManager> MeetingManagers => Set<MeetingManager>();
    public DbSet<MeetingCandidate> MeetingCandidates => Set<MeetingCandidate>();
    public DbSet<MeetingGuest> MeetingGuests => Set<MeetingGuest>();
    public DbSet<MeetingAttachment> MeetingAttachments => Set<MeetingAttachment>();
    public DbSet<MeetingImage> MeetingImages => Set<MeetingImage>();
    public DbSet<MeetingApproval> MeetingApprovals => Set<MeetingApproval>();
    public DbSet<MeetingRepeatDraft> MeetingRepeatDrafts => Set<MeetingRepeatDraft>();
    public DbSet<VoteSession> VoteSessions => Set<VoteSession>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Decision> Decisions => Set<Decision>();
    public DbSet<MeetingMinute> MeetingMinutes => Set<MeetingMinute>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FileAssetLink> FileAssetLinks => Set<FileAssetLink>();
    public DbSet<SavedQueryView> SavedQueryViews => Set<SavedQueryView>();
    public DbSet<EmailOutbox> EmailOutbox => Set<EmailOutbox>();
    public DbSet<SystemModule> SystemModules => Set<SystemModule>();
    public DbSet<SystemPageGroup> SystemPageGroups => Set<SystemPageGroup>();
    public DbSet<SystemPage> SystemPages => Set<SystemPage>();
    public DbSet<MemberProfile> MemberProfiles => Set<MemberProfile>();
    public DbSet<MembershipType> MembershipTypes => Set<MembershipType>();
    public DbSet<MemberPayment> MemberPayments => Set<MemberPayment>();
    public DbSet<MemberCard> MemberCards => Set<MemberCard>();
    public DbSet<MemberReportShare> MemberReportShares => Set<MemberReportShare>();
    public DbSet<MemberParticipationAssignment> MemberParticipationAssignments => Set<MemberParticipationAssignment>();
    public DbSet<ManagementTask> ManagementTasks => Set<ManagementTask>();
    public DbSet<ManagementTaskActivity> ManagementTaskActivities => Set<ManagementTaskActivity>();
    public DbSet<ApprovalRoute> ApprovalRoutes => Set<ApprovalRoute>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalAction> ApprovalActions => Set<ApprovalAction>();
    public DbSet<InternalMailMessage> InternalMailMessages => Set<InternalMailMessage>();
    public DbSet<InternalMailRecipient> InternalMailRecipients => Set<InternalMailRecipient>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<SystemNotification> SystemNotifications => Set<SystemNotification>();
    public DbSet<SystemNotificationRecipient> SystemNotificationRecipients => Set<SystemNotificationRecipient>();
    public DbSet<ChannelDeliveryLog> ChannelDeliveryLogs => Set<ChannelDeliveryLog>();
    public DbSet<AppPermission> AppPermissions => Set<AppPermission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserLoginAudit> UserLoginAudits => Set<UserLoginAudit>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();
    public DbSet<FileAssetVersion> FileAssetVersions => Set<FileAssetVersion>();
    public DbSet<EmployeeDepartment> EmployeeDepartments => Set<EmployeeDepartment>();
    public DbSet<JobTitle> JobTitles => Set<JobTitle>();
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<EmployeeAttendance> EmployeeAttendanceRecords => Set<EmployeeAttendance>();
    public DbSet<EmployeeLeaveRequest> EmployeeLeaveRequests => Set<EmployeeLeaveRequest>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmployeeDisciplinaryRecord> EmployeeDisciplinaryRecords => Set<EmployeeDisciplinaryRecord>();
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances => Set<EmployeeLeaveBalance>();
    public DbSet<EmployeeEvaluation> EmployeeEvaluations => Set<EmployeeEvaluation>();
    public DbSet<EmployeeCardIssue> EmployeeCardIssues => Set<EmployeeCardIssue>();
    public DbSet<EmployeeLetterRequest> EmployeeLetterRequests => Set<EmployeeLetterRequest>();
    public DbSet<EmployeePayrollRecord> EmployeePayrollRecords => Set<EmployeePayrollRecord>();
    public DbSet<EmployeeAttendancePolicy> EmployeeAttendancePolicies => Set<EmployeeAttendancePolicy>();
    public DbSet<EmployeeAttendanceLocation> EmployeeAttendanceLocations => Set<EmployeeAttendanceLocation>();
    public DbSet<EmployeeOfficialVacation> EmployeeOfficialVacations => Set<EmployeeOfficialVacation>();
    public DbSet<EmployeeAttendanceExcuse> EmployeeAttendanceExcuses => Set<EmployeeAttendanceExcuse>();
    public DbSet<HrSafetyCategory> HrSafetyCategories => Set<HrSafetyCategory>();
    public DbSet<HrSafetyProcedure> HrSafetyProcedures => Set<HrSafetyProcedure>();
    public DbSet<HrSafetyInspection> HrSafetyInspections => Set<HrSafetyInspection>();
    public DbSet<RecruitmentRequest> RecruitmentRequests => Set<RecruitmentRequest>();
    public DbSet<EmployeeAdministrativeRequest> EmployeeAdministrativeRequests => Set<EmployeeAdministrativeRequest>();
    public DbSet<HumanResourceActivity> HumanResourceActivities => Set<HumanResourceActivity>();
    public DbSet<ProgramProject> ProgramProjects => Set<ProgramProject>();
    public DbSet<ProgramProjectTask> ProgramProjectTasks => Set<ProgramProjectTask>();
    public DbSet<ProgramProjectMilestone> ProgramProjectMilestones => Set<ProgramProjectMilestone>();
    public DbSet<ProgramProjectContract> ProgramProjectContracts => Set<ProgramProjectContract>();
    public DbSet<ProgramProjectFinanceEntry> ProgramProjectFinanceEntries => Set<ProgramProjectFinanceEntry>();
    public DbSet<ProgramProjectAssignment> ProgramProjectAssignments => Set<ProgramProjectAssignment>();
    public DbSet<ProgramProjectReport> ProgramProjectReports => Set<ProgramProjectReport>();
    public DbSet<ProgramProjectActivity> ProgramProjectActivities => Set<ProgramProjectActivity>();
    public DbSet<ProgramSupplier> ProgramSuppliers => Set<ProgramSupplier>();
    public DbSet<ProgramSupplierProposal> ProgramSupplierProposals => Set<ProgramSupplierProposal>();
    public DbSet<ProgramIdea> ProgramIdeas => Set<ProgramIdea>();
    public DbSet<ProgramApproval> ProgramApprovals => Set<ProgramApproval>();
    public DbSet<ProgramRegistration> ProgramRegistrations => Set<ProgramRegistration>();
    public DbSet<ProgramSession> ProgramSessions => Set<ProgramSession>();
    public DbSet<ProgramSessionAttendance> ProgramSessionAttendanceRecords => Set<ProgramSessionAttendance>();
    public DbSet<ProgramSurvey> ProgramSurveys => Set<ProgramSurvey>();
    public DbSet<ProgramSurveySubmission> ProgramSurveySubmissions => Set<ProgramSurveySubmission>();
    public DbSet<ProgramCertificateTemplate> ProgramCertificateTemplates => Set<ProgramCertificateTemplate>();
    public DbSet<ProgramCertificateIssue> ProgramCertificateIssues => Set<ProgramCertificateIssue>();
    public DbSet<ProgramQualificationCase> ProgramQualificationCases => Set<ProgramQualificationCase>();
    public DbSet<ProgramQualificationInstallment> ProgramQualificationInstallments => Set<ProgramQualificationInstallment>();
    public DbSet<BeneficiaryProfile> BeneficiaryProfiles => Set<BeneficiaryProfile>();
    public DbSet<BeneficiaryDependent> BeneficiaryDependents => Set<BeneficiaryDependent>();
    public DbSet<BeneficiaryGuardian> BeneficiaryGuardians => Set<BeneficiaryGuardian>();
    public DbSet<BeneficiaryUpdateRequest> BeneficiaryUpdateRequests => Set<BeneficiaryUpdateRequest>();
    public DbSet<BeneficiaryEntity> BeneficiaryEntities => Set<BeneficiaryEntity>();
    public DbSet<BeneficiaryAccountArtifact> BeneficiaryAccountArtifacts => Set<BeneficiaryAccountArtifact>();
    public DbSet<BeneficiaryGuardianOperation> BeneficiaryGuardianOperations => Set<BeneficiaryGuardianOperation>();
    public DbSet<BeneficiaryUpdateBatch> BeneficiaryUpdateBatches => Set<BeneficiaryUpdateBatch>();
    public DbSet<BeneficiaryAidRequest> BeneficiaryAidRequests => Set<BeneficiaryAidRequest>();
    public DbSet<BeneficiaryPaymentOrder> BeneficiaryPaymentOrders => Set<BeneficiaryPaymentOrder>();
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();
    public DbSet<SponsorshipRequirement> SponsorshipRequirements => Set<SponsorshipRequirement>();
    public DbSet<SponsorshipRecord> SponsorshipRecords => Set<SponsorshipRecord>();
    public DbSet<SponsorshipPayment> SponsorshipPayments => Set<SponsorshipPayment>();
    public DbSet<EntitySupportRequest> EntitySupportRequests => Set<EntitySupportRequest>();
    public DbSet<CouponRequest> CouponRequests => Set<CouponRequest>();
    public DbSet<AccountingSetting> AccountingSettings => Set<AccountingSetting>();
    public DbSet<FinanceBankAccount> FinanceBankAccounts => Set<FinanceBankAccount>();
    public DbSet<FinanceAccount> FinanceAccounts => Set<FinanceAccount>();
    public DbSet<FinanceCostCenter> FinanceCostCenters => Set<FinanceCostCenter>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<LedgerLine> LedgerLines => Set<LedgerLine>();
    public DbSet<ReceiptVoucher> ReceiptVouchers => Set<ReceiptVoucher>();
    public DbSet<DeferredReceivable> DeferredReceivables => Set<DeferredReceivable>();
    public DbSet<ExpenseVoucher> ExpenseVouchers => Set<ExpenseVoucher>();
    public DbSet<SalaryDisbursement> SalaryDisbursements => Set<SalaryDisbursement>();
    public DbSet<FinancialReviewItem> FinancialReviewItems => Set<FinancialReviewItem>();
    public DbSet<FinanceBudget> FinanceBudgets => Set<FinanceBudget>();
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
    public DbSet<MediaPartner> MediaPartners => Set<MediaPartner>();
    public DbSet<MediaEvent> MediaEvents => Set<MediaEvent>();
    public DbSet<MediaVisit> MediaVisits => Set<MediaVisit>();
    public DbSet<WebsiteUserAccount> WebsiteUserAccounts => Set<WebsiteUserAccount>();
    public DbSet<CommunicationTemplate> CommunicationTemplates => Set<CommunicationTemplate>();
    public DbSet<CommunicationList> CommunicationLists => Set<CommunicationList>();
    public DbSet<CommunicationCampaign> CommunicationCampaigns => Set<CommunicationCampaign>();
    public DbSet<CommunicationCampaignRecipient> CommunicationCampaignRecipients => Set<CommunicationCampaignRecipient>();
    public DbSet<PushSubscriber> PushSubscribers => Set<PushSubscriber>();
    public DbSet<WebsiteDesignSetting> WebsiteDesignSettings => Set<WebsiteDesignSetting>();
    public DbSet<WebsiteNavigationItem> WebsiteNavigationItems => Set<WebsiteNavigationItem>();
    public DbSet<WebsiteContentItem> WebsiteContentItems => Set<WebsiteContentItem>();
    public DbSet<WebsiteForm> WebsiteForms => Set<WebsiteForm>();
    public DbSet<WebsiteFormSubmission> WebsiteFormSubmissions => Set<WebsiteFormSubmission>();
    public DbSet<WebsiteContactRequest> WebsiteContactRequests => Set<WebsiteContactRequest>();
    public DbSet<FinancialSupporter> FinancialSupporters => Set<FinancialSupporter>();
    public DbSet<FundraisingOpportunity> FundraisingOpportunities => Set<FundraisingOpportunity>();
    public DbSet<DonationContribution> DonationContributions => Set<DonationContribution>();
    public DbSet<DonationContributionActivity> DonationContributionActivities => Set<DonationContributionActivity>();
    public DbSet<DigitalMarketingCampaign> DigitalMarketingCampaigns => Set<DigitalMarketingCampaign>();
    public DbSet<AbandonedDonationCart> AbandonedDonationCarts => Set<AbandonedDonationCart>();
    public DbSet<EndowmentAsset> EndowmentAssets => Set<EndowmentAsset>();
    public DbSet<EndowmentContract> EndowmentContracts => Set<EndowmentContract>();
    public DbSet<EndowmentInvoice> EndowmentInvoices => Set<EndowmentInvoice>();
    public DbSet<PerformanceMeasure> PerformanceMeasures => Set<PerformanceMeasure>();
    public DbSet<GovernanceCycle> GovernanceCycles => Set<GovernanceCycle>();
    public DbSet<GovernanceCriterion> GovernanceCriteria => Set<GovernanceCriterion>();
    public DbSet<GovernanceAttachment> GovernanceAttachments => Set<GovernanceAttachment>();
    public DbSet<GovernanceTask> GovernanceTasks => Set<GovernanceTask>();
    public DbSet<StrategicPlan> StrategicPlans => Set<StrategicPlan>();
    public DbSet<StrategicPerspective> StrategicPerspectives => Set<StrategicPerspective>();
    public DbSet<StrategicGoal> StrategicGoals => Set<StrategicGoal>();
    public DbSet<StrategicIndicator> StrategicIndicators => Set<StrategicIndicator>();
    public DbSet<StrategicVariable> StrategicVariables => Set<StrategicVariable>();
    public DbSet<FollowUpCase> FollowUpCases => Set<FollowUpCase>();
    public DbSet<FollowUpActivity> FollowUpActivities => Set<FollowUpActivity>();
    public DbSet<FleetVehicle> FleetVehicles => Set<FleetVehicle>();
    public DbSet<VehicleRequest> VehicleRequests => Set<VehicleRequest>();
    public DbSet<VehicleAssignment> VehicleAssignments => Set<VehicleAssignment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<SystemReportDefinition> SystemReportDefinitions => Set<SystemReportDefinition>();
    public DbSet<SystemReportRun> SystemReportRuns => Set<SystemReportRun>();
    public DbSet<ArchiveDocument> ArchiveDocuments => Set<ArchiveDocument>();
    public DbSet<CorrespondenceRecord> CorrespondenceRecords => Set<CorrespondenceRecord>();
    public DbSet<CorrespondenceOperation> CorrespondenceOperations => Set<CorrespondenceOperation>();
    public DbSet<VolunteerUser> VolunteerUsers => Set<VolunteerUser>();
    public DbSet<VolunteerRequest> VolunteerRequests => Set<VolunteerRequest>();
    public DbSet<VolunteerOpportunity> VolunteerOpportunities => Set<VolunteerOpportunity>();
    public DbSet<VolunteerOpportunityTask> VolunteerOpportunityTasks => Set<VolunteerOpportunityTask>();
    public DbSet<VolunteerAttendanceRecord> VolunteerAttendanceRecords => Set<VolunteerAttendanceRecord>();
    public DbSet<TechSystemSetting> TechSystemSettings => Set<TechSystemSetting>();
    public DbSet<OrganizationAssignment> OrganizationAssignments => Set<OrganizationAssignment>();
    public DbSet<VisualAssetTemplate> VisualAssetTemplates => Set<VisualAssetTemplate>();
    public DbSet<CybersecurityReview> CybersecurityReviews => Set<CybersecurityReview>();
    public DbSet<NcnpDataRecord> NcnpDataRecords => Set<NcnpDataRecord>();
    public DbSet<EstablishmentDocument> EstablishmentDocuments => Set<EstablishmentDocument>();
    public DbSet<AidCommitteeCreditEntry> AidCommitteeCreditEntries => Set<AidCommitteeCreditEntry>();
    public DbSet<ExecutiveApprovalRequest> ExecutiveApprovalRequests => Set<ExecutiveApprovalRequest>();
    public DbSet<PaymentAuthorization> PaymentAuthorizations => Set<PaymentAuthorization>();
    public DbSet<AdministrativeDecisionRecord> AdministrativeDecisionRecords => Set<AdministrativeDecisionRecord>();
    public DbSet<OfficeAttendanceEntry> OfficeAttendanceEntries => Set<OfficeAttendanceEntry>();
    public DbSet<OfficeReminder> OfficeReminders => Set<OfficeReminder>();
    public DbSet<OfficeAdministrativeRequest> OfficeAdministrativeRequests => Set<OfficeAdministrativeRequest>();
    public DbSet<OfficeTransaction> OfficeTransactions => Set<OfficeTransaction>();
    public DbSet<OfficeLogRecord> OfficeLogRecords => Set<OfficeLogRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(x => typeof(IAuditable).IsAssignableFrom(x.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(IAuditable.CreatedByUserId)).HasMaxLength(450);
            modelBuilder.Entity(entityType.ClrType).Property(nameof(IAuditable.UpdatedByUserId)).HasMaxLength(450);
        }
    }

    public override int SaveChanges()
    {
        var pendingAudits = CaptureEntityAudits();
        ApplyAuditValues();
        var result = base.SaveChanges();
        SaveEntityAudits(pendingAudits);
        return result;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesWithAuditsAsync(cancellationToken);
    }

    private async Task<int> SaveChangesWithAuditsAsync(CancellationToken cancellationToken)
    {
        var pendingAudits = CaptureEntityAudits();
        ApplyAuditValues();
        var result = await base.SaveChangesAsync(cancellationToken);
        await SaveEntityAuditsAsync(pendingAudits, cancellationToken);
        return result;
    }

    private void ApplyAuditValues()
    {
        var now = DateTime.UtcNow.AddHours(3);
        var userId = currentUserContext?.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = now;

                entry.Entity.CreatedByUserId ??= userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedByUserId = userId;

                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Property(x => x.CreatedByUserId).IsModified = false;
            }
        }
    }

    private List<PendingEntityAudit> CaptureEntityAudits()
    {
        ChangeTracker.DetectChanges();
        var pending = new List<PendingEntityAudit>();
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.Entity is AuditLog or UserLoginAudit || entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => null
            };
            if (action is null) continue;

            var before = entry.State is EntityState.Modified or EntityState.Deleted
                ? SerializeValues(entry, originalValues: true)
                : null;
            var after = entry.State is EntityState.Added or EntityState.Modified
                ? SerializeValues(entry, originalValues: false)
                : null;
            var entityId = entry.State == EntityState.Added ? null : GetEntityId(entry);
            pending.Add(new PendingEntityAudit(entry.Entity, entry.Metadata.ClrType.Name, action, entityId, before, after));
        }
        return pending;
    }

    private void SaveEntityAudits(IEnumerable<PendingEntityAudit> pendingAudits)
    {
        var audits = CreateAuditLogs(pendingAudits);
        if (audits.Count == 0) return;
        AuditLogs.AddRange(audits);
        ApplyAuditValues();
        base.SaveChanges();
    }

    private async Task SaveEntityAuditsAsync(IEnumerable<PendingEntityAudit> pendingAudits, CancellationToken cancellationToken)
    {
        var audits = CreateAuditLogs(pendingAudits);
        if (audits.Count == 0) return;
        await AuditLogs.AddRangeAsync(audits, cancellationToken);
        ApplyAuditValues();
        await base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> CreateAuditLogs(IEnumerable<PendingEntityAudit> pendingAudits) => pendingAudits.Select(x => new AuditLog
    {
        ActorUserId = currentUserContext?.UserId ?? "system",
        Action = x.Action,
        EntityName = x.EntityName,
        EntityId = x.EntityId ?? GetEntityId(Entry(x.Entity)),
        Details = $"Automatic entity change capture; ip={currentUserContext?.RemoteIpAddress ?? "unknown"}.",
        BeforeJson = x.BeforeJson,
        AfterJson = x.AfterJson
    }).ToList();

    private static string GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return string.Empty;
        return string.Join("|", key.Properties.Select(x => Convert.ToString(entry.Property(x.Name).CurrentValue) ?? string.Empty));
    }

    private static string SerializeValues(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, bool originalValues)
    {
        var values = entry.Properties
            .Where(x => !IsAuditInfrastructureProperty(x.Metadata.Name))
            .Where(x => entry.State != EntityState.Modified || x.IsModified || x.Metadata.IsPrimaryKey())
            .ToDictionary(
                x => x.Metadata.Name,
                x => originalValues ? x.OriginalValue : x.CurrentValue);
        return JsonSerializer.Serialize(values);
    }

    private static bool IsAuditInfrastructureProperty(string propertyName) => propertyName is "CreatedAt" or "CreatedByUserId" or "UpdatedAt" or "UpdatedByUserId"
        || propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("SecurityStamp", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase)
        || propertyName.Contains("Secret", StringComparison.OrdinalIgnoreCase);

    private sealed record PendingEntityAudit(object Entity, string EntityName, string Action, string? EntityId, string? BeforeJson, string? AfterJson);
}
