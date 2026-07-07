using Domain.Entities;

namespace Application.Contracts.SystemCatalog;

public record SystemModuleResponse(
    int Id,
    string Key,
    string NameAr,
    string NameEn,
    string Description,
    int Priority,
    int TotalPages,
    int PlannedPages,
    int InProgressPages,
    int ImplementedPages);

public record SystemPageResponse(
    int Id,
    int ModuleId,
    string ModuleKey,
    string ModuleName,
    string Key,
    string NameAr,
    string Route,
    string PermissionKey,
    string ServiceName,
    string ServicePlan,
    string UiPlan,
    string Status,
    int SortOrder);

public record SeedSystemCatalogResponse(int ModulesCreated, int PagesCreated, int PagesUpdated, int TotalPages);

public record UpdateSystemPageStatusRequest(SystemPageStatus Status);
