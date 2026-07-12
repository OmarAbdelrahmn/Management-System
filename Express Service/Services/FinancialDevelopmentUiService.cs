using Application.Contracts.FinancialDevelopment;
using Application.Service.FinancialDevelopment;
using Domain.Entities;

namespace Express_Service.Services;

public class FinancialDevelopmentUiService(IFinancialDevelopmentService service)
{
    public async Task<FinancialDevelopmentDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<FinancialSupporterResponse>> GetSupportersAsync(FinancialSupporterStatus? status = null, string? search = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetSupportersAsync(status, search, cancellationToken));

    public async Task<(bool Success, string Message)> SaveSupporterAsync(int? id, SaveFinancialSupporterRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveSupporterAsync(id, request, cancellationToken), "تم حفظ حساب الداعم.");

    public async Task<List<FundraisingOpportunityResponse>> GetOpportunitiesAsync(FundraisingOpportunityType? type = null, FundraisingOpportunityStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetOpportunitiesAsync(type, status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveOpportunityAsync(int? id, SaveFundraisingOpportunityRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveOpportunityAsync(id, request, cancellationToken), "تم حفظ فرصة تنمية الموارد.");

    public async Task<(bool Success, string Message)> CompleteOpportunityAsync(int id, string? notes = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.CompleteOpportunityAsync(id, new CompleteFundraisingOpportunityRequest(notes), cancellationToken), "تم إغلاق فرصة تنمية الموارد كمكتملة.");

    public async Task<List<DonationContributionResponse>> GetContributionsAsync(DonationContributionStatus? status = null, int? supporterId = null, int? opportunityId = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetContributionsAsync(status, supporterId, opportunityId, cancellationToken));

    public async Task<(bool Success, string Message)> SaveContributionAsync(int? id, SaveDonationContributionRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveContributionAsync(id, request, cancellationToken), "تم حفظ التبرع.");

    public async Task<(bool Success, string Message)> UpdateContributionStatusAsync(int id, DonationContributionStatus status, string? notes = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.UpdateContributionStatusAsync(id, new UpdateDonationContributionStatusRequest(status, notes), cancellationToken), "تم تحديث حالة التبرع.");

    public async Task<List<DonationContributionActivityResponse>> GetContributionActivitiesAsync(int contributionId, CancellationToken cancellationToken = default) =>
        ToList(await service.GetContributionActivitiesAsync(contributionId, cancellationToken));

    public async Task<DonationReportSummaryResponse?> GetDonationReportAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetDonationReportAsync(from, to, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<DigitalMarketingCampaignResponse>> GetDigitalCampaignsAsync(DigitalMarketingCampaignStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetDigitalCampaignsAsync(status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveDigitalCampaignAsync(int? id, SaveDigitalMarketingCampaignRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveDigitalCampaignAsync(id, request, cancellationToken), "تم حفظ الحملة التسويقية.");

    public async Task<(bool Success, string Message)> RecordDigitalCampaignDonationAsync(int id, RecordDigitalCampaignDonationRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.RecordDigitalCampaignDonationAsync(id, request, cancellationToken), "تم تسجيل تبرع من الحملة وربطه بالتقارير.");

    public async Task<List<AbandonedDonationCartResponse>> GetAbandonedCartsAsync(AbandonedDonationCartStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetAbandonedCartsAsync(status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveAbandonedCartAsync(int? id, SaveAbandonedDonationCartRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveAbandonedCartAsync(id, request, cancellationToken), "تم حفظ السلة المتروكة.");

    public async Task<(bool Success, string Message)> RecoverAbandonedCartAsync(int id, RecoverAbandonedDonationCartRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.RecoverAbandonedCartAsync(id, request, cancellationToken), "تم تحويل السلة المتروكة إلى تبرع.");

    public async Task<List<EndowmentAssetResponse>> GetEndowmentsAsync(EndowmentAssetStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetEndowmentsAsync(status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveEndowmentAsync(int? id, SaveEndowmentAssetRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveEndowmentAsync(id, request, cancellationToken), "تم حفظ الوقف.");

    public async Task<List<EndowmentContractResponse>> GetEndowmentContractsAsync(int? endowmentAssetId = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetEndowmentContractsAsync(endowmentAssetId, cancellationToken));

    public async Task<(bool Success, string Message)> SaveEndowmentContractAsync(int? id, SaveEndowmentContractRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveEndowmentContractAsync(id, request, cancellationToken), "تم حفظ عقد الوقف.");

    public async Task<List<EndowmentInvoiceResponse>> GetEndowmentInvoicesAsync(EndowmentInvoiceStatus? status = null, bool dueSoonOnly = false, CancellationToken cancellationToken = default) =>
        ToList(await service.GetEndowmentInvoicesAsync(status, dueSoonOnly, cancellationToken));

    public async Task<(bool Success, string Message)> SaveEndowmentInvoiceAsync(int? id, SaveEndowmentInvoiceRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveEndowmentInvoiceAsync(id, request, cancellationToken), "تم حفظ دفعة الوقف.");

    public async Task<(bool Success, string Message)> PayEndowmentInvoiceAsync(int id, PayEndowmentInvoiceRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.PayEndowmentInvoiceAsync(id, request, cancellationToken), "تم تحصيل دفعة الوقف وإنشاء سند قبض.");

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
