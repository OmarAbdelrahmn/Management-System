using Application.Contracts.ExecutiveSupervision;
using Application.Service.ExecutiveSupervision;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class ExecutiveSupervisionServiceTests
{
    [Fact]
    public async Task FoundationAndAidCommittee_UpdateDashboard()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ExecutiveSupervisionService(dbcontext);

        var document = await service.SaveFoundationDocumentAsync(null, new SaveEstablishmentDocumentRequest("FD-1", "خارطة الملفات", "الإدارة", null, EstablishmentDocumentStatus.Active, "مكتمل"));
        var credit = await service.SaveAidCommitteeEntryAsync(null, new SaveAidCommitteeCreditEntryRequest("AC-1", AidCommitteeCreditType.Allocation, 1000, DateTime.UtcNow.AddHours(3), null, null));
        var expense = await service.SaveAidCommitteeEntryAsync(null, new SaveAidCommitteeCreditEntryRequest("AC-2", AidCommitteeCreditType.Expense, 250, DateTime.UtcNow.AddHours(3), null, null));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(document.IsSuccess);
        Assert.True(credit.IsSuccess);
        Assert.True(expense.IsSuccess);
        Assert.Equal(1, dashboard.Value.FoundationDocumentsCount);
        Assert.Equal(750, dashboard.Value.AidCommitteeBalance);
    }

    [Fact]
    public async Task ApprovalsAndAuthorizations_CanBeDecided()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ExecutiveSupervisionService(dbcontext);

        var approval = await service.SaveApprovalAsync(null, new SaveExecutiveApprovalRequestRequest("APR-1", ExecutiveApprovalKind.PaymentOrder, "أمر صرف", 500, "مدير", ExecutiveWorkflowStatus.Pending, null, DateTime.UtcNow.AddHours(3)));
        var authorization = await service.SavePaymentAuthorizationAsync(null, new SavePaymentAuthorizationRequest("AUTH-1", "مورد", "شراء", 300, ExecutiveWorkflowStatus.Pending, null, DateTime.UtcNow.AddHours(3)));
        var approvalDecision = await service.DecideApprovalAsync(approval.Value.Id, new DecideExecutiveWorkflowRequest(ExecutiveWorkflowStatus.Approved, "معتمد"));
        var authorizationDecision = await service.DecidePaymentAuthorizationAsync(authorization.Value.Id, new DecideExecutiveWorkflowRequest(ExecutiveWorkflowStatus.FinalRejected, "مرفوض"));
        var approved = await service.GetApprovalsAsync(status: ExecutiveWorkflowStatus.Approved);
        var rejected = await service.GetPaymentAuthorizationsAsync(ExecutiveWorkflowStatus.FinalRejected);

        Assert.True(approvalDecision.IsSuccess);
        Assert.True(authorizationDecision.IsSuccess);
        Assert.Single(approved.Value);
        Assert.Single(rejected.Value);
    }

    [Fact]
    public async Task AdministrativeDecisions_AreSavedByType()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ExecutiveSupervisionService(dbcontext);

        var saved = await service.SaveAdministrativeDecisionAsync(null, new SaveAdministrativeDecisionRecordRequest("DEC-1", AdministrativeDecisionType.Meeting, "قرار اجتماع", "MT-1", "المشرف", ExecutiveWorkflowStatus.Approved, DateTime.UtcNow.AddHours(3), "نموذج", null));
        var meetings = await service.GetAdministrativeDecisionsAsync(AdministrativeDecisionType.Meeting);

        Assert.True(saved.IsSuccess);
        Assert.Single(meetings.Value);
    }
}
