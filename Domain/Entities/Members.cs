using Domain.Auditing;
using Domain.Identity;

namespace Domain.Entities;

public enum MemberStatus
{
    Active = 0,
    Suspended = 1,
    Cancelled = 2
}

public enum MemberPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2
}

public enum MemberParticipationRole
{
    BoardMember = 0,
    GeneralAssembly = 1
}

public enum MemberParticipationStatus
{
    Active = 0,
    Ended = 1,
    Suspended = 2
}

public class MembershipType : IAuditable
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public decimal AnnualFee { get; set; }
    public decimal VotingWeight { get; set; } = 1;
    public bool IsSupporterType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<MemberProfile> Members { get; set; } = new List<MemberProfile>();
}

public class MemberProfile : IAuditable
{
    public int Id { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int MembershipTypeId { get; set; }
    public string? ApplicationUserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public bool FeesPaid { get; set; }
    public bool IsSupporter { get; set; }
    public decimal CumulativePercentage { get; set; } = 1;
    public string? Notes { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public MembershipType? MembershipType { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    public ICollection<MemberPayment> Payments { get; set; } = new List<MemberPayment>();
    public ICollection<MemberCard> Cards { get; set; } = new List<MemberCard>();
    public ICollection<MemberParticipationAssignment> ParticipationAssignments { get; set; } = new List<MemberParticipationAssignment>();
}

public class MemberPayment : IAuditable
{
    public int Id { get; set; }
    public int MemberProfileId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public MemberPaymentStatus Status { get; set; } = MemberPaymentStatus.Pending;
    public string? ReceiptNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public MemberProfile? MemberProfile { get; set; }
}

public class MemberCard : IAuditable
{
    public int Id { get; set; }
    public int MemberProfileId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public MemberProfile? MemberProfile { get; set; }
}

public class MemberReportShare : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Audience { get; set; } = "AllActiveMembers";
    public DateTime SharedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public int RecipientCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class MemberParticipationAssignment : IAuditable
{
    public int Id { get; set; }
    public int MemberProfileId { get; set; }
    public MemberParticipationRole Role { get; set; }
    public string? PositionTitle { get; set; }
    public string? CycleName { get; set; }
    public DateTime StartsAt { get; set; } = DateTime.UtcNow.AddHours(3).Date;
    public DateTime? EndsAt { get; set; }
    public MemberParticipationStatus Status { get; set; } = MemberParticipationStatus.Active;
    public decimal VotingWeight { get; set; } = 1;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public MemberProfile? MemberProfile { get; set; }
}
