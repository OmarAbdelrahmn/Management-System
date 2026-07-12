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
        var closedAidRequests = await service.GetAidRequestsAsync(AidRequestStatus.Closed, null);

        Assert.True(aid.IsSuccess);
        Assert.StartsWith("AID-2026-", aid.Value.RequestNumber);
        Assert.Equal("CommitteeApproved", decision.Value.Status);
        Assert.StartsWith("ORD-2026-", order.Value.OrderNumber);
        Assert.Equal("Closed", orderDecision.Value.Status);
        Assert.NotNull(orderDecision.Value.ClosedAt);
        Assert.Single(closedAidRequests.Value);
    }

    [Fact]
    public async Task CreatePaymentOrderFromAidRequest_TransfersApprovedAidAndPreventsDuplicates()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var beneficiary = await SeedBeneficiaryAsync(dbcontext);
        var service = new BeneficiaryServicesService(dbcontext);

        var aid = await service.SaveAidRequestAsync(null, new SaveAidRequestRequest(beneficiary.Id, null, null, "إعانة سكن", 1800, "دعم إيجار", false));
        await service.DecideAidRequestAsync(aid.Value.Id, new DecideAidRequestRequest(AidRequestStatus.CommitteeApproved, null, "معتمد للصرف"));

        var order = await service.CreatePaymentOrderFromAidRequestAsync(aid.Value.Id, new CreatePaymentOrderFromAidRequestRequest(PaymentOrderType.Finance, new DateTime(2026, 7, 25), "أمر صرف من الطلب"));
        var duplicate = await service.CreatePaymentOrderFromAidRequestAsync(aid.Value.Id, new CreatePaymentOrderFromAidRequestRequest(PaymentOrderType.Finance, null, null));
        var transferred = await service.GetAidRequestsAsync(AidRequestStatus.Transferred, null);
        var orders = await service.GetPaymentOrdersAsync(PaymentOrderType.Finance, PaymentOrderStatus.Pending);

        Assert.True(order.IsSuccess);
        Assert.StartsWith("ORD-2026-", order.Value.OrderNumber);
        Assert.Equal(aid.Value.Id, order.Value.BeneficiaryAidRequestId);
        Assert.Equal(1800, order.Value.Amount);
        Assert.True(duplicate.IsFailure);
        Assert.Single(transferred.Value);
        Assert.Single(orders.Value);
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
    public async Task GenerateSponsorshipPaymentsAsync_CreatesScheduleAndPreventsDuplicates()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var beneficiary = await SeedBeneficiaryAsync(dbcontext);
        var service = new BeneficiaryServicesService(dbcontext);
        var sponsor = await service.SaveSponsorAsync(null, new SaveSponsorRequest("كافل جدولة", "0500000001", null, SponsorStatus.Active, null));
        var requirement = await service.SaveSponsorshipRequirementAsync(null, new SaveSponsorshipRequirementRequest("كفالة تعليمية", 500, "Monthly", SponsorshipStatus.Active, null));
        var record = await service.SaveSponsorshipRecordAsync(null, new SaveSponsorshipRecordRequest(sponsor.Value.Id, beneficiary.Id, requirement.Value.Id, new DateTime(2026, 7, 1), new DateTime(2026, 10, 1), 500, SponsorshipStatus.Active, null));

        var generated = await service.GenerateSponsorshipPaymentsAsync(record.Value.Id, new GenerateSponsorshipPaymentsRequest(new DateTime(2026, 8, 1), 3, 1, "دفعات شهرية"));
        var duplicate = await service.GenerateSponsorshipPaymentsAsync(record.Value.Id, new GenerateSponsorshipPaymentsRequest(new DateTime(2026, 8, 1), 3, 1, null));
        var records = await service.GetSponsorshipRecordsAsync(SponsorshipStatus.Active);
        var pendingPayments = await service.GetSponsorshipPaymentsAsync(SponsorshipPaymentStatus.Pending);

        Assert.True(generated.IsSuccess);
        var payments = generated.Value.ToList();
        Assert.Equal(3, payments.Count);
        Assert.Equal([new DateTime(2026, 8, 1), new DateTime(2026, 9, 1), new DateTime(2026, 10, 1)], payments.Select(x => x.DueDate).ToArray());
        Assert.All(payments, x => Assert.Equal(500, x.Amount));
        Assert.True(duplicate.IsFailure);
        Assert.Equal(1500, Assert.Single(records.Value).PendingAmount);
        Assert.Equal(3, pendingPayments.Value.Count());
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
        var directDelivery = await service.UpdateCouponStatusAsync(coupon.Value.Id, new UpdateCouponStatusRequest(CouponStatus.Delivered, "تسليم مباشر"));
        var issued = await service.UpdateCouponStatusAsync(coupon.Value.Id, new UpdateCouponStatusRequest(CouponStatus.Issued, "تم الإصدار"));
        var approved = await service.UpdateCouponStatusAsync(coupon.Value.Id, new UpdateCouponStatusRequest(CouponStatus.Approved, "تم الاعتماد"));
        var delivered = await service.UpdateCouponStatusAsync(coupon.Value.Id, new UpdateCouponStatusRequest(CouponStatus.Delivered, "تم التسليم"));

        Assert.True(supportDecision.IsSuccess);
        Assert.Equal("Approved", supportDecision.Value.Status);
        Assert.True(directDelivery.IsFailure);
        Assert.True(issued.IsSuccess);
        Assert.NotNull(issued.Value.IssuedAt);
        Assert.True(approved.IsSuccess);
        Assert.True(delivered.IsSuccess);
        Assert.Equal("Delivered", delivered.Value.Status);
        Assert.NotNull(delivered.Value.DeliveredAt);
    }

    [Fact]
    public async Task CreatePaymentOrderFromEntitySupport_RequiresApprovalAndClosesSupportWithOrder()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var entity = new BeneficiaryEntity { NameAr = "جمعية شريكة" };
        dbcontext.BeneficiaryEntities.Add(entity);
        await dbcontext.SaveChangesAsync();
        var service = new BeneficiaryServicesService(dbcontext);
        var support = await service.SaveEntitySupportAsync(null, new SaveEntitySupportRequest(entity.Id, "ممثل الجمعية", "دعم برنامج", 4200, false));

        var beforeApproval = await service.CreatePaymentOrderFromEntitySupportAsync(support.Value.Id, new CreatePaymentOrderFromEntitySupportRequest(PaymentOrderType.Finance, new DateTime(2026, 7, 28), null));
        await service.DecideEntitySupportAsync(support.Value.Id, new DecideEntitySupportRequest(EntitySupportStatus.Approved, "معتمد للصرف"));
        var order = await service.CreatePaymentOrderFromEntitySupportAsync(support.Value.Id, new CreatePaymentOrderFromEntitySupportRequest(PaymentOrderType.Finance, new DateTime(2026, 7, 28), "أمر صرف جهة"));
        var duplicate = await service.CreatePaymentOrderFromEntitySupportAsync(support.Value.Id, new CreatePaymentOrderFromEntitySupportRequest(PaymentOrderType.Finance, null, null));
        var closedOrder = await service.DecidePaymentOrderAsync(order.Value.Id, new DecidePaymentOrderRequest(PaymentOrderStatus.Closed, "تم الصرف"));
        var closedSupports = await service.GetEntitySupportsAsync(EntitySupportStatus.Closed, false);

        Assert.True(beforeApproval.IsFailure);
        Assert.True(order.IsSuccess);
        Assert.StartsWith("ORD-2026-", order.Value.OrderNumber);
        Assert.Equal(support.Value.Id, order.Value.EntitySupportRequestId);
        Assert.Equal("ممثل الجمعية", order.Value.EntitySupportRequesterName);
        Assert.Equal(4200, order.Value.Amount);
        Assert.True(duplicate.IsFailure);
        Assert.True(closedOrder.IsSuccess);
        var closedSupport = Assert.Single(closedSupports.Value);
        Assert.Equal(support.Value.Id, closedSupport.Id);
        Assert.Equal("تم الصرف", closedSupport.DecisionNotes);
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
