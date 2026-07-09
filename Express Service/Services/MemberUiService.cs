using Application.Contracts.Members;
using Application.Service.Members;
using Domain.Entities;

namespace Express_Service.Services;

public class MemberUiService(IMemberService memberService)
{
    public async Task<List<MembershipTypeResponse>> GetTypesAsync(CancellationToken cancellationToken = default)
    {
        var result = await memberService.GetMembershipTypesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<MemberResponse>> GetMembersAsync(MemberStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await memberService.SearchAsync(new MemberSearchRequest(null, status, null, null, null), cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<MemberResponse>> SearchMembersAsync(string? search, MemberStatus? status, int? typeId, bool? feesPaid, bool? isSupporter, CancellationToken cancellationToken = default)
    {
        var result = await memberService.SearchAsync(new MemberSearchRequest(search, status, typeId, feesPaid, isSupporter), cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<MemberResponse?> GetMemberAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await memberService.GetAsync(id, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<(bool Success, string Message, MemberResponse? Member)> CreateMemberAsync(CreateMemberRequest request, CancellationToken cancellationToken = default)
    {
        var result = await memberService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة العضو بنجاح.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<(bool Success, string Message, MemberResponse? Member)> UpdateMemberAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken = default)
    {
        var result = await memberService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث بيانات العضو.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<(bool Success, string Message)> CancelMemberAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var result = await memberService.CancelAsync(id, new CancelMemberRequest(reason), cancellationToken);
        return result.IsSuccess ? (true, "تم إلغاء العضوية.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RestoreMemberAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await memberService.RestoreAsync(id, cancellationToken);
        return result.IsSuccess ? (true, "تمت استعادة العضوية.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message, MembershipTypeResponse? Type)> SaveTypeAsync(int? id, UpsertMembershipTypeRequest request, CancellationToken cancellationToken = default)
    {
        var result = id.HasValue
            ? await memberService.UpdateMembershipTypeAsync(id.Value, request, cancellationToken)
            : await memberService.CreateMembershipTypeAsync(request, cancellationToken);

        return result.IsSuccess ? (true, "تم حفظ فئة العضوية.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<List<MemberPaymentResponse>> GetPaymentsAsync(int? memberId = null, CancellationToken cancellationToken = default)
    {
        var result = await memberService.GetPaymentsAsync(memberId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<MemberResponse>> GetDueMembersAsync(CancellationToken cancellationToken = default)
    {
        var result = await memberService.GetDueMembersAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> RecordPaymentAsync(int memberId, RecordMemberPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await memberService.RecordPaymentAsync(memberId, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل السداد.") : (false, result.Error.Description);
    }

    public async Task<List<MemberCardResponse>> GetCardsAsync(CancellationToken cancellationToken = default)
    {
        var result = await memberService.GetCardsAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> IssueCardAsync(int memberId, DateTime? expiresAt, CancellationToken cancellationToken = default)
    {
        var result = await memberService.IssueCardAsync(memberId, new IssueMemberCardRequest(expiresAt), cancellationToken);
        return result.IsSuccess ? (true, "تم إصدار بطاقة العضو.") : (false, result.Error.Description);
    }

    public async Task<List<MemberReportShareResponse>> GetReportSharesAsync(CancellationToken cancellationToken = default)
    {
        var result = await memberService.GetReportSharesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> ShareReportAsync(ShareMemberReportRequest request, CancellationToken cancellationToken = default)
    {
        var result = await memberService.ShareReportAsync(request, cancellationToken);
        return result.IsSuccess ? (true, $"تمت مشاركة التقرير مع {result.Value.RecipientCount} عضو.") : (false, result.Error.Description);
    }

    public async Task<List<MemberParticipationResponse>> GetParticipationAsync(
        MemberParticipationRole? role = null,
        MemberParticipationStatus? status = null,
        int? memberProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await memberService.GetParticipationAssignmentsAsync(new MemberParticipationSearchRequest(role, status, memberProfileId), cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveParticipationAsync(int? id, SaveMemberParticipationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await memberService.SaveParticipationAssignmentAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ تصنيف العضو المشارك.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> EndParticipationAsync(int id, DateTime? endsAt, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await memberService.EndParticipationAssignmentAsync(id, new EndMemberParticipationRequest(endsAt, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم إنهاء تصنيف العضو المشارك.") : (false, result.Error.Description);
    }
}
