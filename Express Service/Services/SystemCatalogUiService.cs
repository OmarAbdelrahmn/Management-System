using Application.Contracts.SystemCatalog;
using Application.Service.SystemCatalog;
using Domain.Entities;

namespace Express_Service.Services;

public class SystemCatalogUiService(ISystemCatalogService systemCatalogService)
{
    public async Task<(bool Success, string Message)> SeedSystemCatalogAsync(CancellationToken cancellationToken = default)
    {
        var result = await systemCatalogService.SeedFirstPagesAsync(cancellationToken);
        return result.IsSuccess
            ? (true, $"تم تجهيز {result.Value.TotalPages} صفحة ضمن {result.Value.TotalGroups} مجموعة و {result.Value.TotalModules} إدارة. جديد: {result.Value.PagesCreated} صفحة، تحديث: {result.Value.PagesUpdated}.")
            : (false, result.Error.Description);
    }

    public async Task<List<SystemModuleResponse>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var result = await systemCatalogService.GetModulesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<SystemPageGroupResponse>> GetGroupsAsync(string? moduleKey = null, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var result = await systemCatalogService.GetGroupsAsync(moduleKey, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<SystemPageResponse>> GetPagesAsync(string? moduleKey = null, string? status = null, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var result = await systemCatalogService.GetPagesAsync(moduleKey, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<SystemPageResponse?> GetPageAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var result = await systemCatalogService.GetPageAsync(id, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<SystemPageResponse?> GetPageByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var result = await systemCatalogService.GetPageByKeyAsync(key, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<SystemNavigationModuleResponse>> GetNavigationAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var result = await systemCatalogService.GetNavigationAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<SystemNavigationPageResponse>> SearchAccessiblePagesAsync(string? search, int take = 8, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        var term = search.Trim();
        var navigation = await GetNavigationAsync(cancellationToken);
        return navigation
            .SelectMany(x => x.Groups)
            .SelectMany(x => x.Pages)
            .Where(x => x.NameAr.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        x.Key.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        x.Route.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Take(Math.Clamp(take, 1, 20))
            .ToList();
    }

    public async Task<CatalogRouteAccessResponse> GetRouteAccessAsync(string route, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var result = await systemCatalogService.GetRouteAccessAsync(route, cancellationToken);
        return result.IsSuccess ? result.Value : new CatalogRouteAccessResponse(false, true, []);
    }

    public async Task<(bool Success, string Message)> UpdatePageStatusAsync(int id, SystemPageStatus status, CancellationToken cancellationToken = default)
    {
        var result = await systemCatalogService.UpdatePageStatusAsync(id, new UpdateSystemPageStatusRequest(status), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة الصفحة.") : (false, result.Error.Description);
    }

    private async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        var navigation = await systemCatalogService.GetNavigationAsync(cancellationToken);
        if (navigation.IsSuccess && navigation.Value.Any())
            return;

        await systemCatalogService.SeedFirstPagesAsync(cancellationToken);
    }
}
