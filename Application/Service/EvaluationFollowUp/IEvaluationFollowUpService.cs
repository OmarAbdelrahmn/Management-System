using Application.Abstraction;
using Application.Contracts.EvaluationFollowUp;
using Domain.Entities;

namespace Application.Service.EvaluationFollowUp;

public interface IEvaluationFollowUpService
{
    Task<Result<EvaluationFollowUpDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FollowUpCaseResponse>>> GetCasesAsync(FollowUpCaseStatus? status = null, FollowUpSubjectType? subjectType = null, CancellationToken cancellationToken = default);
    Task<Result<FollowUpCaseResponse>> SaveCaseAsync(int? id, SaveFollowUpCaseRequest request, CancellationToken cancellationToken = default);
    Task<Result<FollowUpCaseResponse>> UpdateCaseStatusAsync(int id, UpdateFollowUpCaseStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FollowUpActivityResponse>>> GetActivitiesAsync(int? caseId = null, FollowUpSubjectType? subjectType = null, bool nextActionsOnly = false, CancellationToken cancellationToken = default);
    Task<Result<FollowUpActivityResponse>> SaveActivityAsync(int? id, SaveFollowUpActivityRequest request, CancellationToken cancellationToken = default);
}
