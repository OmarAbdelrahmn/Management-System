namespace Domain.Entities;

public enum BoardStatus
{
    Active = 1,
    Closed = 2
}

public enum MeetingStatus
{
    Draft = 1,
    InvitationsSent = 2,
    WaitingForResponses = 3,
    InProgress = 4,
    WaitingChairmanApproval = 5,
    ApprovedAndArchived = 6,
    Cancelled = 7,
    PendingApproval = 8,
    Rejected = 9,
    Finished = 10
}

public enum MeetingType
{
    General = 0,
    Board = 1,
    Assembly = 2
}

public enum MeetingImportance
{
    Normal = 0,
    Important = 1,
    Urgent = 2
}

public enum MeetingRepeatMode
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4
}

public enum MeetingApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Expired = 3
}

public enum MeetingNoteVisibility
{
    BeforeCouncil = 1,
    DuringAgendaItem = 2
}

public enum VoteChoice
{
    Approve = 1,
    Reject = 2,
    Abstain = 3
}

public enum VoteSessionStatus
{
    Open = 1,
    Closed = 2
}

public enum AgendaItemStatus
{
    Pending = 1,
    DecisionApproved = 2,
    Rejected = 3
}

public enum DecisionStatus
{
    WaitingChairmanSignature = 1,
    Published = 2
}
