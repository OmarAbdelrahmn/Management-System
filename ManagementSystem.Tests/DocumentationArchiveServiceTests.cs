using Application.Contracts.DocumentationArchive;
using Application.Service.DocumentationArchive;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class DocumentationArchiveServiceTests
{
    [Fact]
    public async Task SaveArchiveDocumentAsync_CreatesDocumentAndDashboardCounts()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new DocumentationArchiveService(dbcontext);

        var saved = await service.SaveArchiveDocumentAsync(null, new SaveArchiveDocumentRequest("DOC-1", "لائحة الحفظ", ArchiveDocumentCategory.Secret, "/files/doc-1.pdf", "الإدارة", ArchiveDocumentStatus.Active, "سري"));
        var documents = await service.GetArchiveDocumentsAsync(ArchiveDocumentCategory.Secret);
        var dashboard = await service.GetDashboardAsync();

        Assert.True(saved.IsSuccess);
        Assert.Single(documents.Value);
        Assert.Equal(1, dashboard.Value.ArchiveDocumentsCount);
        Assert.Equal(1, dashboard.Value.ActiveDocumentsCount);
    }

    [Fact]
    public async Task SaveCorrespondenceAsync_UpdatesStatusAndFiltersMail()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new DocumentationArchiveService(dbcontext);

        var saved = await service.SaveCorrespondenceAsync(null, new SaveCorrespondenceRecordRequest("MAIL-1", CorrespondenceDirection.Incoming, "خطاب وارد", "المركز الوطني", DateTime.UtcNow.AddHours(3), "BC-1", CorrespondenceStatus.Registered, null));
        var update = await service.UpdateCorrespondenceStatusAsync(saved.Value.Id, new UpdateCorrespondenceStatusRequest(CorrespondenceStatus.NeedsReply, "بانتظار الرد"));
        var needsReply = await service.GetCorrespondenceAsync(status: CorrespondenceStatus.NeedsReply);

        Assert.True(saved.IsSuccess);
        Assert.True(update.IsSuccess);
        Assert.Single(needsReply.Value);
        Assert.Equal(CorrespondenceStatus.NeedsReply.ToString(), needsReply.Value.Single().Status);
    }

    [Fact]
    public async Task SaveOperationAsync_CompletesMailOperation()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new DocumentationArchiveService(dbcontext);
        var mail = await service.SaveCorrespondenceAsync(null, new SaveCorrespondenceRecordRequest("MAIL-2", CorrespondenceDirection.Outgoing, "صادر متابعة", "جهة خارجية", DateTime.UtcNow.AddHours(3), null, CorrespondenceStatus.Registered, null));

        var operation = await service.SaveOperationAsync(null, new SaveCorrespondenceOperationRequest(mail.Value.Id, "OP-1", "متابعة الرد", "السكرتير", DateTime.UtcNow.AddHours(3).AddDays(3), CorrespondenceOperationStatus.Open, null));
        var needsReply = await service.GetCorrespondenceAsync(status: CorrespondenceStatus.NeedsReply);
        var complete = await service.CompleteOperationAsync(operation.Value.Id, new CompleteCorrespondenceOperationRequest(DateTime.UtcNow.AddHours(3), "تم الإكمال"));
        var completed = await service.GetOperationsAsync(CorrespondenceOperationStatus.Completed);
        var replied = await service.GetCorrespondenceAsync(status: CorrespondenceStatus.Completed);

        Assert.True(operation.IsSuccess);
        Assert.Single(needsReply.Value);
        Assert.True(complete.IsSuccess);
        Assert.Single(completed.Value);
        Assert.Equal("MAIL-2", completed.Value.Single().MailNumber);
        Assert.Single(replied.Value);
    }

    [Fact]
    public async Task CorrespondenceRemovalWorkflow_RequiresClosedOperationsAndApproval()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new DocumentationArchiveService(dbcontext);
        var mail = await service.SaveCorrespondenceAsync(null, new SaveCorrespondenceRecordRequest("MAIL-3", CorrespondenceDirection.Incoming, "وارد للحذف", "جهة خارجية", DateTime.UtcNow.AddHours(3), null, CorrespondenceStatus.Registered, null));
        var operation = await service.SaveOperationAsync(null, new SaveCorrespondenceOperationRequest(mail.Value.Id, "OP-DEL-1", "متابعة قبل الحذف", "السكرتير", DateTime.UtcNow.AddHours(3).AddDays(1), CorrespondenceOperationStatus.Open, null));

        var blockedByOpenOperation = await service.RequestCorrespondenceRemovalAsync(mail.Value.Id, new RequestCorrespondenceRemovalRequest("طلب حذف مبكر"));
        await service.CompleteOperationAsync(operation.Value.Id, new CompleteCorrespondenceOperationRequest(DateTime.UtcNow.AddHours(3), "أغلقت المعاملة"));
        var pendingRemoval = await service.RequestCorrespondenceRemovalAsync(mail.Value.Id, new RequestCorrespondenceRemovalRequest("طلب حذف بعد الإغلاق"));
        var rejectedRemoval = await service.DecideCorrespondenceRemovalAsync(mail.Value.Id, new DecideCorrespondenceRemovalRequest(false, "رفض الحذف"));
        var secondPendingRemoval = await service.RequestCorrespondenceRemovalAsync(mail.Value.Id, new RequestCorrespondenceRemovalRequest("طلب حذف ثان"));
        var approvedRemoval = await service.DecideCorrespondenceRemovalAsync(mail.Value.Id, new DecideCorrespondenceRemovalRequest(true, "اعتماد الحذف"));
        var operationAfterRemoval = await service.SaveOperationAsync(null, new SaveCorrespondenceOperationRequest(mail.Value.Id, "OP-DEL-2", "معاملة بعد الحذف", "السكرتير", null, CorrespondenceOperationStatus.Open, null));
        var removed = await service.GetCorrespondenceAsync(status: CorrespondenceStatus.Removed);

        Assert.True(operation.IsSuccess);
        Assert.False(blockedByOpenOperation.IsSuccess);
        Assert.True(pendingRemoval.IsSuccess);
        Assert.Equal("PendingRemovalApproval", pendingRemoval.Value.Status);
        Assert.True(rejectedRemoval.IsSuccess);
        Assert.Equal("Registered", rejectedRemoval.Value.Status);
        Assert.True(secondPendingRemoval.IsSuccess);
        Assert.True(approvedRemoval.IsSuccess);
        Assert.Equal("Removed", approvedRemoval.Value.Status);
        Assert.False(operationAfterRemoval.IsSuccess);
        Assert.Equal(mail.Value.Id, Assert.Single(removed.Value).Id);
    }
}
