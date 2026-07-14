using Domain.Entities;

namespace Application.Contracts.Members;

public record MembershipTypeResponse(
    int Id,
    string NameAr,
    string? NameEn,
    decimal AnnualFee,
    decimal VotingWeight,
    bool IsSupporterType,
    bool IsActive);

public record UpsertMembershipTypeRequest(
    string NameAr,
    string? NameEn,
    decimal AnnualFee,
    decimal VotingWeight,
    bool IsSupporterType,
    bool IsActive = true);

public record MemberResponse(
    int Id,
    string MemberNumber,
    string FullName,
    string? NationalId,
    string? Email,
    string? Mobile,
    string? City,
    string? Address,
    int MembershipTypeId,
    string MembershipTypeName,
    string? ApplicationUserId,
    DateTime JoinedAt,
    string Status,
    bool FeesPaid,
    bool IsSupporter,
    decimal CumulativePercentage,
    string? Notes,
    DateTime? CancelledAt,
    string? CancellationReason,
    decimal TotalPaid,
    DateTime CreatedAt);

public record CreateMemberRequest(
    string FullName,
    int MembershipTypeId,
    string? MemberNumber,
    string? NationalId,
    string? Email,
    string? Mobile,
    string? City,
    string? Address,
    string? ApplicationUserId,
    DateTime? JoinedAt,
    bool FeesPaid,
    bool IsSupporter,
    decimal CumulativePercentage,
    string? Notes);

public record UpdateMemberRequest(
    string FullName,
    int MembershipTypeId,
    string? NationalId,
    string? Email,
    string? Mobile,
    string? City,
    string? Address,
    string? ApplicationUserId,
    DateTime JoinedAt,
    bool FeesPaid,
    bool IsSupporter,
    decimal CumulativePercentage,
    string? Notes);

public record MemberSearchRequest(
    string? Search,
    MemberStatus? Status,
    int? MembershipTypeId,
    bool? FeesPaid,
    bool? IsSupporter);

public record CancelMemberRequest(string Reason);

public record MemberPaymentResponse(
    int Id,
    int MemberProfileId,
    string MemberName,
    decimal Amount,
    DateTime DueDate,
    DateTime? PaidAt,
    string Status,
    string? ReceiptNumber,
    string? Notes);

public record RecordMemberPaymentRequest(
    decimal Amount,
    DateTime DueDate,
    DateTime? PaidAt,
    string? ReceiptNumber,
    string? Notes,
    bool MarkAsPaid = true);

public record CancelMemberPaymentRequest(string Reason);

public record SettleMemberPaymentRequest(DateTime? PaidAt, string? ReceiptNumber, string? Notes);

public record MemberCardResponse(
    int Id,
    int MemberProfileId,
    string MemberName,
    string CardNumber,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    bool IsActive);

public record IssueMemberCardRequest(DateTime? ExpiresAt);

public record ShareMemberReportRequest(string Title, string Body, string Audience);

public record MemberReportShareResponse(
    int Id,
    string Title,
    string Body,
    string Audience,
    DateTime SharedAt,
    int RecipientCount);

public record MemberParticipationResponse(
    int Id,
    int MemberProfileId,
    string MemberNumber,
    string MemberName,
    string Role,
    string? PositionTitle,
    string? CycleName,
    DateTime StartsAt,
    DateTime? EndsAt,
    string Status,
    decimal VotingWeight,
    string? Notes);

public record MemberParticipationSearchRequest(
    MemberParticipationRole? Role,
    MemberParticipationStatus? Status,
    int? MemberProfileId);

public record SaveMemberParticipationRequest(
    int MemberProfileId,
    MemberParticipationRole Role,
    string? PositionTitle,
    string? CycleName,
    DateTime StartsAt,
    DateTime? EndsAt,
    MemberParticipationStatus Status,
    decimal VotingWeight,
    string? Notes);

public record EndMemberParticipationRequest(
    DateTime? EndsAt,
    string? Notes);
