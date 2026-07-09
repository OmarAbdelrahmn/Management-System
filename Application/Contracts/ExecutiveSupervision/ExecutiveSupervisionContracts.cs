using Domain.Entities;

namespace Application.Contracts.ExecutiveSupervision;

public record ExecutiveSupervisionDashboardResponse(int FoundationDocumentsCount, decimal AidCommitteeBalance, int PendingApprovalsCount, int PendingPaymentAuthorizationsCount, int AdministrativeDecisionsCount);
public record EstablishmentDocumentResponse(int Id, string DocumentCode, string Title, string? OwnerDepartment, string? FileUrl, string Status, string? HelperNotes);
public record SaveEstablishmentDocumentRequest(string DocumentCode, string Title, string? OwnerDepartment, string? FileUrl, EstablishmentDocumentStatus Status, string? HelperNotes);
public record AidCommitteeCreditEntryResponse(int Id, string EntryNumber, string EntryType, decimal Amount, DateTime EntryDate, string? Reference, string? Notes);
public record SaveAidCommitteeCreditEntryRequest(string EntryNumber, AidCommitteeCreditType EntryType, decimal Amount, DateTime EntryDate, string? Reference, string? Notes);
public record ExecutiveApprovalRequestResponse(int Id, string RequestNumber, string ApprovalKind, string Subject, decimal Amount, string RequestedBy, string Status, string? DecisionNotes, DateTime RequestedAt, DateTime? DecidedAt);
public record SaveExecutiveApprovalRequestRequest(string RequestNumber, ExecutiveApprovalKind ApprovalKind, string Subject, decimal Amount, string RequestedBy, ExecutiveWorkflowStatus Status, string? DecisionNotes, DateTime RequestedAt);
public record DecideExecutiveWorkflowRequest(ExecutiveWorkflowStatus Status, string? DecisionNotes);
public record PaymentAuthorizationResponse(int Id, string AuthorizationNumber, string PayeeName, string Purpose, decimal Amount, string Status, string? RejectionNote, DateTime AuthorizationDate);
public record SavePaymentAuthorizationRequest(string AuthorizationNumber, string PayeeName, string Purpose, decimal Amount, ExecutiveWorkflowStatus Status, string? RejectionNote, DateTime AuthorizationDate);
public record AdministrativeDecisionRecordResponse(int Id, string DecisionNumber, string DecisionType, string Title, string? RelatedMeetingCode, string? AssignedTo, string Status, DateTime DecisionDate, string? ExportTemplateName, string? Notes);
public record SaveAdministrativeDecisionRecordRequest(string DecisionNumber, AdministrativeDecisionType DecisionType, string Title, string? RelatedMeetingCode, string? AssignedTo, ExecutiveWorkflowStatus Status, DateTime DecisionDate, string? ExportTemplateName, string? Notes);
