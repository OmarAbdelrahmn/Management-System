using Application.Abstraction;
using Application.Contracts.SystemCatalog;

namespace Application.Service.SystemCatalog;

public interface ISystemCatalogService
{
    Task<Result<SeedSystemCatalogResponse>> SeedFirstPagesAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SystemModuleResponse>>> GetModulesAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SystemPageResponse>>> GetPagesAsync(string? moduleKey = null, string? status = null, CancellationToken cancellationToken = default);
    Task<Result<SystemPageResponse>> GetPageAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> UpdatePageStatusAsync(int id, UpdateSystemPageStatusRequest request, CancellationToken cancellationToken = default);
}
