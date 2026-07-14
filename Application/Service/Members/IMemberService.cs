using Application.Abstraction;
using Application.Contracts.Members;

namespace Application.Service.Members;

public interface IMemberService
{
    Task<Result<IEnumerable<MembershipTypeResponse>>> GetMembershipTypesAsync(CancellationToken cancellationToken = default);
    Task<Result<MembershipTypeResponse>> CreateMembershipTypeAsync(UpsertMembershipTypeRequest request, CancellationToken cancellationToken = default);
    Task<Result<MembershipTypeResponse>> UpdateMembershipTypeAsync(int id, UpsertMembershipTypeRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MemberResponse>>> SearchAsync(MemberSearchRequest request, CancellationToken cancellationToken = default);
    Task<Result<MemberResponse>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<MemberResponse>> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken = default);
    Task<Result<MemberResponse>> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(int id, CancelMemberRequest request, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<MemberPaymentResponse>> RecordPaymentAsync(int memberId, RecordMemberPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result> SettlePaymentAsync(int id, SettleMemberPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelPaymentAsync(int id, CancelMemberPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MemberPaymentResponse>>> GetPaymentsAsync(int? memberId = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MemberResponse>>> GetDueMembersAsync(CancellationToken cancellationToken = default);
    Task<Result<MemberCardResponse>> IssueCardAsync(int memberId, IssueMemberCardRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateCardAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MemberCardResponse>>> GetCardsAsync(CancellationToken cancellationToken = default);
    Task<Result<MemberReportShareResponse>> ShareReportAsync(ShareMemberReportRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MemberReportShareResponse>>> GetReportSharesAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MemberParticipationResponse>>> GetParticipationAssignmentsAsync(MemberParticipationSearchRequest request, CancellationToken cancellationToken = default);
    Task<Result<MemberParticipationResponse>> SaveParticipationAssignmentAsync(int? id, SaveMemberParticipationRequest request, CancellationToken cancellationToken = default);
    Task<Result<MemberParticipationResponse>> EndParticipationAssignmentAsync(int id, EndMemberParticipationRequest request, CancellationToken cancellationToken = default);
}
