using Application.Contracts.Members;
using Application.Service.Members;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManagementSystem.Tests;

public class MemberServiceTests
{
    [Fact]
    public async Task SaveParticipationAssignmentAsync_CreatesAndFiltersBoardAssignment()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MemberService(dbcontext);
        var member = await SeedMemberAsync(dbcontext);

        var created = await service.SaveParticipationAssignmentAsync(null, new SaveMemberParticipationRequest(
            member.Id,
            MemberParticipationRole.BoardMember,
            "رئيس المجلس",
            "دورة 2026",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            MemberParticipationStatus.Active,
            2,
            "عضو مؤسس"));
        var boardAssignments = await service.GetParticipationAssignmentsAsync(new MemberParticipationSearchRequest(MemberParticipationRole.BoardMember, MemberParticipationStatus.Active, null));

        Assert.True(created.IsSuccess);
        Assert.Equal("BoardMember", created.Value.Role);
        Assert.Equal("رئيس المجلس", created.Value.PositionTitle);
        var assignment = Assert.Single(boardAssignments.Value);
        Assert.Equal(member.MemberNumber, assignment.MemberNumber);
    }

    [Fact]
    public async Task SaveParticipationAssignmentAsync_RejectsDuplicateActiveRole()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MemberService(dbcontext);
        var member = await SeedMemberAsync(dbcontext);
        var request = new SaveMemberParticipationRequest(
            member.Id,
            MemberParticipationRole.GeneralAssembly,
            null,
            "الجمعية 2026",
            new DateTime(2026, 1, 1),
            null,
            MemberParticipationStatus.Active,
            1,
            null);

        var first = await service.SaveParticipationAssignmentAsync(null, request);
        var duplicate = await service.SaveParticipationAssignmentAsync(null, request);

        Assert.True(first.IsSuccess);
        Assert.True(duplicate.IsFailure);
    }

    [Fact]
    public async Task EndParticipationAssignmentAsync_EndsAssignmentAndKeepsHistory()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MemberService(dbcontext);
        var member = await SeedMemberAsync(dbcontext);
        var created = await service.SaveParticipationAssignmentAsync(null, new SaveMemberParticipationRequest(
            member.Id,
            MemberParticipationRole.BoardMember,
            "عضو",
            "دورة 2026",
            new DateTime(2026, 1, 1),
            null,
            MemberParticipationStatus.Active,
            1,
            null));

        var ended = await service.EndParticipationAssignmentAsync(created.Value.Id, new EndMemberParticipationRequest(new DateTime(2026, 6, 30), "انتهاء الدورة"));
        var history = await service.GetParticipationAssignmentsAsync(new MemberParticipationSearchRequest(MemberParticipationRole.BoardMember, null, member.Id));

        Assert.True(ended.IsSuccess);
        Assert.Equal("Ended", ended.Value.Status);
        Assert.Equal(new DateTime(2026, 6, 30), ended.Value.EndsAt);
        var assignment = Assert.Single(history.Value);
        Assert.Equal("Ended", assignment.Status);
    }

    private static async Task<MemberProfile> SeedMemberAsync(ApplicationDbcontext dbcontext)
    {
        var type = new MembershipType
        {
            NameAr = $"عضوية عاملة {Guid.NewGuid():N}",
            AnnualFee = 250,
            VotingWeight = 1,
            IsActive = true
        };
        var member = new MemberProfile
        {
            MemberNumber = $"M-{Guid.NewGuid():N}",
            FullName = "عضو تجريبي",
            MembershipType = type,
            MembershipTypeId = type.Id,
            FeesPaid = true,
            CumulativePercentage = 1
        };

        dbcontext.MembershipTypes.Add(type);
        dbcontext.MemberProfiles.Add(member);
        await dbcontext.SaveChangesAsync();
        return member;
    }
}
