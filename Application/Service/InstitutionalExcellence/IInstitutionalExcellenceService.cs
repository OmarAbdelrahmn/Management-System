using Application.Abstraction;
using Application.Contracts.InstitutionalExcellence;
using Domain.Entities;

namespace Application.Service.InstitutionalExcellence;

public interface IInstitutionalExcellenceService
{
    Task<Result<InstitutionalExcellenceDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PerformanceMeasureResponse>>> GetPerformanceMeasuresAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<PerformanceMeasureResponse>> SavePerformanceMeasureAsync(int? id, SavePerformanceMeasureRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GovernanceCycleResponse>>> GetGovernanceCyclesAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<GovernanceCycleResponse>> SaveGovernanceCycleAsync(int? id, SaveGovernanceCycleRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GovernanceCriterionResponse>>> GetGovernanceCriteriaAsync(int? cycleId = null, GovernanceCriterionStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<GovernanceCriterionResponse>> SaveGovernanceCriterionAsync(int? id, SaveGovernanceCriterionRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GovernanceAttachmentResponse>>> GetGovernanceAttachmentsAsync(int? criterionId = null, CancellationToken cancellationToken = default);
    Task<Result<GovernanceAttachmentResponse>> SaveGovernanceAttachmentAsync(int? id, SaveGovernanceAttachmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GovernanceTaskResponse>>> GetGovernanceTasksAsync(int? cycleId = null, GovernanceTaskStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<GovernanceTaskResponse>> SaveGovernanceTaskAsync(int? id, SaveGovernanceTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<GovernanceReportResponse>> GetGovernanceReportAsync(int? cycleId = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<StrategicPlanResponse>>> GetStrategicPlansAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<StrategicPlanResponse>> SaveStrategicPlanAsync(int? id, SaveStrategicPlanRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<StrategicPerspectiveResponse>>> GetStrategicPerspectivesAsync(int? planId = null, CancellationToken cancellationToken = default);
    Task<Result<StrategicPerspectiveResponse>> SaveStrategicPerspectiveAsync(int? id, SaveStrategicPerspectiveRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<StrategicGoalResponse>>> GetStrategicGoalsAsync(int? perspectiveId = null, CancellationToken cancellationToken = default);
    Task<Result<StrategicGoalResponse>> SaveStrategicGoalAsync(int? id, SaveStrategicGoalRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<StrategicIndicatorResponse>>> GetStrategicIndicatorsAsync(int? planId = null, StrategicIndicatorKind? kind = null, CancellationToken cancellationToken = default);
    Task<Result<StrategicIndicatorResponse>> SaveStrategicIndicatorAsync(int? id, SaveStrategicIndicatorRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<StrategicVariableResponse>>> GetStrategicVariablesAsync(int? planId = null, CancellationToken cancellationToken = default);
    Task<Result<StrategicVariableResponse>> SaveStrategicVariableAsync(int? id, SaveStrategicVariableRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<StrategicVariableResponse>>> FetchAutomatedStrategicVariablesAsync(int planId, CancellationToken cancellationToken = default);
}
