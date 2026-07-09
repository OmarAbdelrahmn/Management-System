using Application.Contracts.BeneficiaryServices;
using Application.Service.BeneficiaryServices;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class BeneficiaryServicesServiceTests
{
    [Fact]
    public async Task AidRequestApprovalAndPaymentOrder_CreateOperationalRecords()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var beneficiary = await SeedBeneficiaryAsync(dbcontext);
        var service = new BeneficiaryServicesService(dbcontext);

        var aid = await service.SaveAidRequestAsync(null, new SaveAidRequestRequest(beneficiary.Id, null, null, "إعانة مالية", 2500, "احتياج طارئ", false));
        var decision = await service.DecideAidRequestAsync(aid.Value.Id, new DecideAidRequestRequest(AidRequestStatus.CommitteeApproved, "مستحق", "معتمد"));
        var order = await service.SavePaymentOrderAsync(null, new SavePaymentOrderRequest(aid.Value.Id, null, null, PaymentOrderType.Finance, 2500, "صرف الإعانة", new DateTime(2026, 7, 15)));
        var orderDecision = await service.DecidePaymentOrderAsync(order.Value.Id, new DecidePaymentOrderRequest(PaymentOrderStatus.Closed, "تم الصرف"));

        Assert.True(aid.IsSuccess);
        Assert.StartsWith("AID-2026-", aid.Value.RequestNumber);
        Assert.Equal("CommitteeApproved", decision.Value.Status);
        Assert.StartsWith("ORD-2026-", order.Value.OrderNumber);
        Assert.Equal("Closed", orderDecision.Value.Status);
        Assert.NotNull(orderDecision.Value.ClosedAt);
    }

    [Fact]
    public async Task SponsorshipWorkflow_CreatesSponsorRecordAndPayment()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var beneficiary = await SeedBeneficiaryAsync(dbcontext);
        var service = new BeneficiaryServicesService(dbcontext);

        var sponsor = await service.SaveSponsorAsync(null, new SaveSponsorRequest("كافل تجريبي", "0500000000", null, SponsorStatus.Active, null));
        var requirement = await service.SaveSponsorshipRequirementAsync(null, new SaveSponsorshipRequirementRequest("كفالة شهرية", 500, "Monthly", SponsorshipStatus.Active, null));
        var record = await service.SaveSponsorshipRecordAsync(null, new SaveSponsorshipRecordRequest(sponsor.Value.Id, beneficiary.Id, requirement.Value.Id, new DateTime(2026, 7, 1), null, 500, SponsorshipStatus.Active, null));
        var payment = await service.SaveSponsorshipPaymentAsync(null, new SaveSponsorshipPaymentRequest(record.Value.Id, new DateTime(2026, 8, 1), 500, SponsorshipPaymentStatus.Paid, null));
        var records = await service.GetSponsorshipRecordsAsync(SponsorshipStatus.Active);

        Assert.True(payment.IsSuccess);
        Assert.Equal("Paid", payment.Value.Status);
        Assert.Equal(500, Assert.Single(records.Value).PaidAmount);
    }

    [Fact]
    public async Task EntitySupportAndCoupons_SaveAndAdvanceStatuses()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var beneficiary = await SeedBeneficiaryAsync(dbcontext);
        var entity = new BeneficiaryEntity { NameAr = "جهة مستفيدة" };
        dbcontext.BeneficiaryEntities.Add(entity);
        await dbcontext.SaveChangesAsync();
        var service = new BeneficiaryServicesService(dbcontext);

        var support = await service.SaveEntitySupportAsync(null, new SaveEntitySupportRequest(entity.Id, "ممثل الجهة", "دعم عيني", 1000, true));
        var supportDecision = await service.DecideEntitySupportAsync(support.Value.Id, new DecideEntitySupportRequest(EntitySupportStatus.Approved, "موافق"));
        var coupon = await service.SaveCouponAsync(null, new SaveCouponRequestRequest(beneficiary.Id, "سلة غذائية", 300, new DateTime(2026, 7, 20), null));
        var delivered = await service.UpdateCouponStatusAsync(coupon.Value.Id, new UpdateCouponStatusRequest(CouponStatus.Delivered, "تم التسليم"));

        Assert.True(supportDecision.IsSuccess);
        Assert.Equal("Approved", supportDecision.Value.Status);
        Assert.True(delivered.IsSuccess);
        Assert.Equal("Delivered", delivered.Value.Status);
        Assert.NotNull(delivered.Value.DeliveredAt);
    }

    private static async Task<BeneficiaryProfile> SeedBeneficiaryAsync(Domain.ApplicationDbcontext dbcontext)
    {
        var beneficiary = new BeneficiaryProfile
        {
            BeneficiaryNumber = $"B-{Guid.NewGuid():N}",
            FullName = "مستفيد تجريبي",
            Status = BeneficiaryStatus.Active,
            FamilyMembersCount = 3
        };

        dbcontext.BeneficiaryProfiles.Add(beneficiary);
        await dbcontext.SaveChangesAsync();
        return beneficiary;
    }
}
