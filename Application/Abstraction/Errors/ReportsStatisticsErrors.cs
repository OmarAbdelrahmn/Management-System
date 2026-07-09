using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class ReportsStatisticsErrors
{
    public static readonly Error DefinitionNotFound = new("ReportsStatistics.DefinitionNotFound", "Report definition was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateDefinition = new("ReportsStatistics.DuplicateDefinition", "Report definition key is already used.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidRequest = new("ReportsStatistics.InvalidRequest", "Reports/statistics request is invalid.", StatusCodes.Status400BadRequest);
}
