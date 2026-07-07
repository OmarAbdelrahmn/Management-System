using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Domain;

public class ApplicationDbcontext(
    DbContextOptions<ApplicationDbcontext> options,
    ICurrentUserContext? currentUserContext = null)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardCycle> BoardCycles => Set<BoardCycle>();
    public DbSet<BoardMembership> BoardMemberships => Set<BoardMembership>();
    public DbSet<BoardMeeting> BoardMeetings => Set<BoardMeeting>();
    public DbSet<MeetingAgendaItem> MeetingAgendaItems => Set<MeetingAgendaItem>();
    public DbSet<MeetingInvitation> MeetingInvitations => Set<MeetingInvitation>();
    public DbSet<MeetingNote> MeetingNotes => Set<MeetingNote>();
    public DbSet<MeetingManager> MeetingManagers => Set<MeetingManager>();
    public DbSet<MeetingCandidate> MeetingCandidates => Set<MeetingCandidate>();
    public DbSet<MeetingGuest> MeetingGuests => Set<MeetingGuest>();
    public DbSet<MeetingAttachment> MeetingAttachments => Set<MeetingAttachment>();
    public DbSet<MeetingImage> MeetingImages => Set<MeetingImage>();
    public DbSet<MeetingApproval> MeetingApprovals => Set<MeetingApproval>();
    public DbSet<MeetingRepeatDraft> MeetingRepeatDrafts => Set<MeetingRepeatDraft>();
    public DbSet<VoteSession> VoteSessions => Set<VoteSession>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Decision> Decisions => Set<Decision>();
    public DbSet<MeetingMinute> MeetingMinutes => Set<MeetingMinute>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EmailOutbox> EmailOutbox => Set<EmailOutbox>();
    public DbSet<SystemModule> SystemModules => Set<SystemModule>();
    public DbSet<SystemPage> SystemPages => Set<SystemPage>();
    public DbSet<MemberProfile> MemberProfiles => Set<MemberProfile>();
    public DbSet<MembershipType> MembershipTypes => Set<MembershipType>();
    public DbSet<MemberPayment> MemberPayments => Set<MemberPayment>();
    public DbSet<MemberCard> MemberCards => Set<MemberCard>();
    public DbSet<MemberReportShare> MemberReportShares => Set<MemberReportShare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(x => typeof(IAuditable).IsAssignableFrom(x.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(IAuditable.CreatedByUserId)).HasMaxLength(450);
            modelBuilder.Entity(entityType.ClrType).Property(nameof(IAuditable.UpdatedByUserId)).HasMaxLength(450);
        }
    }

    public override int SaveChanges()
    {
        ApplyAuditValues();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditValues()
    {
        var now = DateTime.UtcNow.AddHours(3);
        var userId = currentUserContext?.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = now;

                entry.Entity.CreatedByUserId ??= userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedByUserId = userId;

                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Property(x => x.CreatedByUserId).IsModified = false;
            }
        }
    }
}
