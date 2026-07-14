using Domain.Entities;

namespace Application.Contracts.SystemCatalog;

public record SystemModuleResponse(
    int Id,
    string Key,
    string NameAr,
    string NameEn,
    string Description,
    string? IconCss,
    int Priority,
    int TotalGroups,
    int TotalPages,
    int PlannedPages,
    int InProgressPages,
    int ImplementedPages);

public record SystemPageGroupResponse(
    int Id,
    int ModuleId,
    string ModuleKey,
    string Key,
    string NameAr,
    int SortOrder,
    int TotalPages,
    int PlannedPages,
    int InProgressPages,
    int ImplementedPages);

public record SystemPageResponse(
    int Id,
    int ModuleId,
    string ModuleKey,
    string ModuleName,
    int? GroupId,
    string? GroupKey,
    string? GroupName,
    string Key,
    string NameAr,
    string Route,
    string PermissionKey,
    string ServiceName,
    string ServicePlan,
    string UiPlan,
    string? OriginalHref,
    string? OriginalIcon,
    string Status,
    int SortOrder);

public record SystemNavigationModuleResponse(
    string Key,
    string NameAr,
    string Description,
    string? IconCss,
    int SortOrder,
    IReadOnlyList<SystemNavigationGroupResponse> Groups);

public record SystemNavigationGroupResponse(
    string Key,
    string NameAr,
    int SortOrder,
    IReadOnlyList<SystemNavigationPageResponse> Pages);

public record SystemNavigationPageResponse(
    string Key,
    string NameAr,
    string Route,
    string PermissionKey,
    string Status,
    string? OriginalHref,
    string? OriginalIcon,
    int SortOrder);

public record CatalogRouteAccessResponse(
    bool IsCatalogRoute,
    bool IsAllowed,
    IReadOnlyList<string> PermissionKeys);

public record SeedSystemCatalogResponse(
    int ModulesCreated,
    int GroupsCreated,
    int PagesCreated,
    int PagesUpdated,
    int TotalModules,
    int TotalGroups,
    int TotalPages);

public record UpdateSystemPageStatusRequest(SystemPageStatus Status);
