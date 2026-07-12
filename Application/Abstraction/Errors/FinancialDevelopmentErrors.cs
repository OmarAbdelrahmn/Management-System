using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class FinancialDevelopmentErrors
{
    public static readonly Error SupporterNotFound = new("FinancialDevelopment.SupporterNotFound", "Financial supporter was not found.", StatusCodes.Status404NotFound);
    public static readonly Error OpportunityNotFound = new("FinancialDevelopment.OpportunityNotFound", "Fundraising opportunity was not found.", StatusCodes.Status404NotFound);
    public static readonly Error OpportunityNotActive = new("FinancialDevelopment.OpportunityNotActive", "Fundraising opportunity must be active before it can be completed.", StatusCodes.Status409Conflict);
    public static readonly Error OpportunityTargetNotReached = new("FinancialDevelopment.OpportunityTargetNotReached", "Fundraising opportunity target amount has not been reached.", StatusCodes.Status409Conflict);
    public static readonly Error ContributionNotFound = new("FinancialDevelopment.ContributionNotFound", "Donation contribution was not found.", StatusCodes.Status404NotFound);
    public static readonly Error CampaignNotFound = new("FinancialDevelopment.CampaignNotFound", "Digital marketing campaign was not found.", StatusCodes.Status404NotFound);
    public static readonly Error CampaignNotRunning = new("FinancialDevelopment.CampaignNotRunning", "Digital marketing campaign must be running before donations can be attributed.", StatusCodes.Status409Conflict);
    public static readonly Error AbandonedCartNotFound = new("FinancialDevelopment.AbandonedCartNotFound", "Abandoned donation cart was not found.", StatusCodes.Status404NotFound);
    public static readonly Error EndowmentNotFound = new("FinancialDevelopment.EndowmentNotFound", "Endowment asset was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ContractNotFound = new("FinancialDevelopment.ContractNotFound", "Endowment contract was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvoiceNotFound = new("FinancialDevelopment.InvoiceNotFound", "Endowment invoice was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvoiceAlreadyPaid = new("FinancialDevelopment.InvoiceAlreadyPaid", "Endowment invoice is already fully paid.", StatusCodes.Status409Conflict);
    public static readonly Error InvoiceCancelled = new("FinancialDevelopment.InvoiceCancelled", "Cancelled endowment invoice cannot be paid.", StatusCodes.Status409Conflict);
    public static readonly Error PaymentExceedsInvoiceBalance = new("FinancialDevelopment.PaymentExceedsInvoiceBalance", "Payment amount exceeds the remaining endowment invoice balance.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidRequest = new("FinancialDevelopment.InvalidRequest", "Financial development request is invalid.", StatusCodes.Status400BadRequest);
}
