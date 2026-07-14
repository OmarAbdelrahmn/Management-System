using Application.Contracts.Attachments;
using Application.Service.Attachments;
using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;
using Microsoft.Extensions.Options;

namespace ManagementSystem.Tests;

public class AttachmentServiceTests
{
    [Fact]
    public async Task UploadAsync_CreatesCleanVersionAndEntityLink()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-user", UserName = "attachment-user", Email = "attachment@example.test", IsActive = true };
        dbcontext.Users.Add(user); await AddBeneficiaryProfileAsync(dbcontext); await dbcontext.SaveChangesAsync();
        var storage = new FakeStorage();
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id), storage, new FakeScanner(AttachmentMalwareScanResult.Clean));

        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var result = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "HR", "BeneficiaryProfile", "42", "هوية"));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.CurrentVersion);
        Assert.Equal("Clean", result.Value.ScanStatus);
        Assert.Single(result.Value.Links);
        Assert.Single(dbcontext.FileAssetVersions);
        Assert.Single(dbcontext.AuditLogs, x => x.Action == "AttachmentUploaded");
    }

    [Fact]
    public async Task UploadAsync_RejectsInfectedContentWithoutSavingAsset()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-user", UserName = "attachment-user", Email = "attachment@example.test", IsActive = true };
        dbcontext.Users.Add(user); await AddBeneficiaryProfileAsync(dbcontext); await dbcontext.SaveChangesAsync();
        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id), new FakeStorage(), new FakeScanner(AttachmentMalwareScanResult.Infected));

        var result = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "HR", "BeneficiaryProfile", "42", null));

        Assert.True(result.IsFailure);
        Assert.Equal("Attachment.Infected", result.Error.Code);
        Assert.Empty(dbcontext.FileAssets);
    }

    [Fact]
    public async Task UploadAsync_RejectsUserWithoutLinkedEntityPermission()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-user", UserName = "attachment-user", Email = "attachment@example.test", IsActive = true };
        dbcontext.Users.Add(user); await AddBeneficiaryProfileAsync(dbcontext); await dbcontext.SaveChangesAsync();
        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id, ["Operator"]), new FakeStorage(), new FakeScanner(AttachmentMalwareScanResult.Clean));

        var result = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "HR", "BeneficiaryProfile", "42", null));

        Assert.True(result.IsFailure);
        Assert.Equal("Attachment.Forbidden", result.Error.Code);
        Assert.Single(dbcontext.AuditLogs, x => x.Action == "AttachmentUploadDenied");
    }

    [Fact]
    public async Task UploadAsync_AllowsGrantedEntityPermissionWithoutGenericAttachmentPermission()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-user", UserName = "attachment-user", Email = "attachment@example.test", IsActive = true };
        var role = new ApplicationRole { Id = "role-operator", Name = "Operator" };
        var permission = new AppPermission { Key = "system.beneficiary-accounts.profiles_update", NameAr = "تحديث المستفيد", Category = "Beneficiaries" };
        dbcontext.AddRange(user, role, permission);
        await AddBeneficiaryProfileAsync(dbcontext);
        await dbcontext.SaveChangesAsync();
        dbcontext.RolePermissions.Add(new RolePermission { RoleId = role.Id, AppPermissionId = permission.Id, IsGranted = true });
        await dbcontext.SaveChangesAsync();
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id, ["Operator"]), new FakeStorage(), new FakeScanner(AttachmentMalwareScanResult.Clean));

        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var result = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "Beneficiary", "BeneficiaryProfile", "42", null));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UploadAsync_RejectsUnsupportedEntityBeforeSavingTemporaryContent()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-user", UserName = "attachment-user", Email = "attachment@example.test", IsActive = true };
        dbcontext.Users.Add(user); await dbcontext.SaveChangesAsync();
        var storage = new FakeStorage();
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id), storage, new FakeScanner(AttachmentMalwareScanResult.Clean));

        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var result = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "General", "UnknownRecord", "42", null));

        Assert.True(result.IsFailure);
        Assert.Equal("Attachment.UnsupportedEntity", result.Error.Code);
        Assert.Empty(storage.Files);
    }

    [Fact]
    public async Task UploadAsync_UsesConfiguredMaximumSize()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-user", UserName = "attachment-user", Email = "attachment@example.test", IsActive = true };
        dbcontext.Users.Add(user); await AddBeneficiaryProfileAsync(dbcontext); await dbcontext.SaveChangesAsync();
        var service = new AttachmentService(
            dbcontext,
            new TestCurrentUserContext(user.Id),
            new FakeStorage(),
            new FakeScanner(AttachmentMalwareScanResult.Clean),
            Options.Create(new AttachmentOptions { MaximumSizeBytes = 4 }));

        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var result = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "Beneficiary", "BeneficiaryProfile", "42", null));

        Assert.True(result.IsFailure);
        Assert.Equal("Attachment.TooLarge", result.Error.Code);
    }

    [Fact]
    public async Task UploadAsync_ReturnsControlledFailureWhenStorageCannotWrite()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-user", UserName = "attachment-user", Email = "attachment@example.test", IsActive = true };
        dbcontext.Users.Add(user); await AddBeneficiaryProfileAsync(dbcontext); await dbcontext.SaveChangesAsync();
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id), new FailingStorage(), new FakeScanner(AttachmentMalwareScanResult.Clean));

        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var result = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "Beneficiary", "BeneficiaryProfile", "42", null));

        Assert.True(result.IsFailure);
        Assert.Equal("Attachment.MalwareScanFailed", result.Error.Code);
        Assert.Empty(dbcontext.FileAssets);
    }

    [Fact]
    public async Task GetEntityAccessAsync_SeparatesBeneficiaryViewAndManagePermissions()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "attachment-viewer", UserName = "attachment-viewer", Email = "viewer@example.test", IsActive = true };
        var role = new ApplicationRole { Id = "role-viewer", Name = "Viewer" };
        var permission = new AppPermission { Key = "system.beneficiary-accounts.profiles_database", NameAr = "عرض المستفيدين", Category = "Beneficiaries" };
        dbcontext.AddRange(user, role, permission);
        await AddBeneficiaryProfileAsync(dbcontext);
        await dbcontext.SaveChangesAsync();
        dbcontext.RolePermissions.Add(new RolePermission { RoleId = role.Id, AppPermissionId = permission.Id, IsGranted = true });
        await dbcontext.SaveChangesAsync();
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id, ["Viewer"]), new FakeStorage(), new FakeScanner(AttachmentMalwareScanResult.Clean));

        var result = await service.GetEntityAccessAsync("BeneficiaryProfile", "42");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CanView);
        Assert.False(result.Value.CanManage);
    }

    [Fact]
    public async Task GetEntityAccessAsync_AllowsLedgerCreateOrManagePermission()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "ledger-user", UserName = "ledger-user", Email = "ledger@example.test", IsActive = true };
        var role = new ApplicationRole { Id = "role-ledger", Name = "LedgerOperator" };
        var permission = new AppPermission { Key = "system.accounting.finance_ledgers", NameAr = "إضافة قيد يومية", Category = "Accounting" };
        dbcontext.AddRange(user, role, permission, new LedgerEntry { Id = 7, EntryNumber = "J-7", Description = "Test" });
        await dbcontext.SaveChangesAsync();
        dbcontext.RolePermissions.Add(new RolePermission { RoleId = role.Id, AppPermissionId = permission.Id, IsGranted = true });
        await dbcontext.SaveChangesAsync();
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id, ["LedgerOperator"]), new FakeStorage(), new FakeScanner(AttachmentMalwareScanResult.Clean));

        var result = await service.GetEntityAccessAsync("LedgerEntry", "7");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CanView);
        Assert.True(result.Value.CanManage);
    }

    [Fact]
    public async Task ManageOnlyPermission_CanReadAttachmentAfterUpload()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var user = new ApplicationUser { Id = "aid-creator", UserName = "aid-creator", Email = "aid@example.test", IsActive = true };
        var role = new ApplicationRole { Id = "role-aid-creator", Name = "AidCreator" };
        var permission = new AppPermission { Key = "system.beneficiary-services.request_create", NameAr = "إنشاء طلب إعانة", Category = "BeneficiaryServices" };
        var aidRequest = new BeneficiaryAidRequest { Id = 15, RequestNumber = "AID-15", AidType = "مالية", Description = "Test" };
        dbcontext.AddRange(user, role, permission, aidRequest);
        await dbcontext.SaveChangesAsync();
        dbcontext.RolePermissions.Add(new RolePermission { RoleId = role.Id, AppPermissionId = permission.Id, IsGranted = true });
        await dbcontext.SaveChangesAsync();
        var storage = new FakeStorage();
        var service = new AttachmentService(dbcontext, new TestCurrentUserContext(user.Id, ["AidCreator"]), storage, new FakeScanner(AttachmentMalwareScanResult.Clean));

        await using var content = new MemoryStream("%PDF-1.7 test"u8.ToArray());
        var uploaded = await service.UploadAsync(new AttachmentUploadInput(content, "proof.pdf", "application/pdf", content.Length, "Aid", nameof(BeneficiaryAidRequest), "15", null));
        var listed = await service.GetForEntityAsync(nameof(BeneficiaryAidRequest), "15");

        Assert.True(uploaded.IsSuccess);
        Assert.True(listed.IsSuccess);
        Assert.Single(listed.Value);
    }

    private static Task AddBeneficiaryProfileAsync(Domain.ApplicationDbcontext dbcontext)
    {
        dbcontext.BeneficiaryProfiles.Add(new BeneficiaryProfile { Id = 42, BeneficiaryNumber = "B-42", FullName = "Test beneficiary" });
        return Task.CompletedTask;
    }

    private sealed class TestCurrentUserContext(string userId, IReadOnlyCollection<string>? roles = null) : ICurrentUserContext
    {
        public string? UserId { get; } = userId;
        public IReadOnlyCollection<string> Roles { get; } = roles ?? ["Admin"];
    }

    private sealed class FakeScanner(AttachmentMalwareScanResult result) : IAttachmentMalwareScanner
    {
        public Task<AttachmentMalwareScanResult> ScanAsync(string temporaryPath, CancellationToken cancellationToken = default) => Task.FromResult(result);
    }

    private sealed class FakeStorage : IAttachmentStorage
    {
        private readonly Dictionary<string, byte[]> files = [];
        public IReadOnlyDictionary<string, byte[]> Files => files;
        public async Task<string> SaveTemporaryAsync(Stream content, CancellationToken cancellationToken = default) { var key = $"tmp/{Guid.NewGuid():N}"; await using var memory = new MemoryStream(); await content.CopyToAsync(memory, cancellationToken); files[key] = memory.ToArray(); return key; }
        public Task PromoteAsync(string temporaryPath, string permanentPath, CancellationToken cancellationToken = default) { files[permanentPath] = files[temporaryPath]; files.Remove(temporaryPath); return Task.CompletedTask; }
        public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default) => Task.FromResult<Stream?>(files.TryGetValue(storagePath, out var bytes) ? new MemoryStream(bytes) : null);
        public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default) { files.Remove(storagePath); return Task.CompletedTask; }
    }

    private sealed class FailingStorage : IAttachmentStorage
    {
        public Task<string> SaveTemporaryAsync(Stream content, CancellationToken cancellationToken = default) => throw new IOException("Disk unavailable");
        public Task PromoteAsync(string temporaryPath, string permanentPath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default) => Task.FromResult<Stream?>(null);
        public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
