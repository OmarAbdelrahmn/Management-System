using Application.Contracts.HumanResources;
using Application.Service.HumanResources;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class HumanResourceServiceTests
{
    [Fact]
    public async Task CreateEmployeeAsync_GeneratesEmployeeNumberAndMapsLookups()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var department = new EmployeeDepartment { NameAr = "الموارد البشرية" };
        var jobTitle = new JobTitle { NameAr = "أخصائي" };
        dbcontext.EmployeeDepartments.Add(department);
        dbcontext.JobTitles.Add(jobTitle);
        await dbcontext.SaveChangesAsync();

        var service = new HumanResourceService(dbcontext);

        var result = await service.CreateEmployeeAsync(new CreateEmployeeRequest(
            "سارة أحمد",
            department.Id,
            jobTitle.Id,
            null,
            "1002003000",
            "sara@example.com",
            "0500000000",
            new DateTime(2026, 7, 1),
            8000,
            1500,
            null));

        Assert.True(result.IsSuccess);
        Assert.StartsWith("E-2026-", result.Value.EmployeeNumber);
        Assert.Equal("الموارد البشرية", result.Value.DepartmentName);
        Assert.Equal(9500, result.Value.TotalSalary);
    }

    [Fact]
    public async Task RecordAttendanceAsync_RejectsDuplicateEmployeeDate()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);
        var request = new RecordEmployeeAttendanceRequest(employee.Id, new DateTime(2026, 7, 8), new TimeSpan(8, 0, 0), null, AttendanceStatus.Present, null);

        var first = await service.RecordAttendanceAsync(request);
        var duplicate = await service.RecordAttendanceAsync(request);

        Assert.True(first.IsSuccess);
        Assert.True(duplicate.IsFailure);
    }

    [Fact]
    public async Task RecordAttendanceAsync_AppliesDefaultPolicyGraceWindow()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);

        var morningPolicy = await service.SaveAttendancePolicyAsync(null, new SaveAttendancePolicyRequest("دوام صباحي", new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0), 10, "Sunday,Monday,Tuesday,Wednesday,Thursday", true, true));
        var updatedDefaultPolicy = await service.SaveAttendancePolicyAsync(null, new SaveAttendancePolicyRequest("دوام مرن", new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 5, "Sunday,Monday,Tuesday,Wednesday,Thursday", true, true));
        var policies = await service.GetAttendancePoliciesAsync();
        var late = await service.RecordAttendanceAsync(new RecordEmployeeAttendanceRequest(employee.Id, new DateTime(2026, 7, 8), new TimeSpan(9, 6, 0), null, AttendanceStatus.Present, null));
        var onTime = await service.RecordAttendanceAsync(new RecordEmployeeAttendanceRequest(employee.Id, new DateTime(2026, 7, 9), new TimeSpan(9, 5, 0), null, AttendanceStatus.Present, null));

        Assert.True(morningPolicy.IsSuccess);
        Assert.True(updatedDefaultPolicy.IsSuccess);
        Assert.Single(policies.Value, x => x.IsDefault);
        Assert.Equal("Late", late.Value.Status);
        Assert.Equal("Present", onTime.Value.Status);
    }

    [Fact]
    public async Task DecideLeaveRequestAsync_ApprovesLeaveAndMarksEmployeeOnLeave()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);
        var leave = await service.CreateLeaveRequestAsync(new CreateEmployeeLeaveRequest(employee.Id, "سنوية", new DateTime(2026, 7, 10), new DateTime(2026, 7, 12), "سفر"));

        var decision = await service.DecideLeaveRequestAsync(leave.Value.Id, new DecideEmployeeLeaveRequest(true, "موافق"));

        Assert.True(decision.IsSuccess);
        Assert.Equal("Approved", decision.Value.Status);
        Assert.Equal(EmployeeStatus.OnLeave, employee.Status);
    }

    [Fact]
    public async Task SearchEmployeesAsync_FiltersVolunteerAccounts()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var department = new EmployeeDepartment { NameAr = "التطوع" };
        var jobTitle = new JobTitle { NameAr = "متطوع" };
        dbcontext.EmployeeDepartments.Add(department);
        dbcontext.JobTitles.Add(jobTitle);
        await dbcontext.SaveChangesAsync();
        var service = new HumanResourceService(dbcontext);

        var create = await service.CreateEmployeeAsync(new CreateEmployeeRequest(
            "متطوع تجريبي",
            department.Id,
            jobTitle.Id,
            null,
            null,
            null,
            "0501111111",
            new DateTime(2026, 7, 1),
            0,
            0,
            null,
            EmployeeAccountType.Volunteer));

        var volunteers = await service.SearchEmployeesAsync(new EmployeeSearchRequest(null, null, null, null, EmployeeAccountType.Volunteer));

        Assert.True(create.IsSuccess);
        var volunteer = Assert.Single(volunteers.Value);
        Assert.Equal("Volunteer", volunteer.AccountType);
    }

    [Fact]
    public async Task ApprovingLeaveRequest_ConsumesMatchingLeaveBalance()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);
        var balance = await service.SaveLeaveBalanceAsync(null, new SaveEmployeeLeaveBalanceRequest(employee.Id, 2026, "سنوية", 30, 0, 0, null));
        var leave = await service.CreateLeaveRequestAsync(new CreateEmployeeLeaveRequest(employee.Id, "سنوية", new DateTime(2026, 7, 10), new DateTime(2026, 7, 12), null));

        await service.DecideLeaveRequestAsync(leave.Value.Id, new DecideEmployeeLeaveRequest(true, "موافق"));
        var balances = await service.GetLeaveBalancesAsync(employee.Id, 2026);

        Assert.True(balance.IsSuccess);
        Assert.Equal(3, Assert.Single(balances.Value).UsedDays);
    }

    [Fact]
    public async Task ApprovingLeaveRequest_BlocksInsufficientBalanceAndDoubleDecision()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);
        await service.SaveLeaveBalanceAsync(null, new SaveEmployeeLeaveBalanceRequest(employee.Id, 2026, "سنوية", 2, 0, 0, null));
        var tooLongLeave = await service.CreateLeaveRequestAsync(new CreateEmployeeLeaveRequest(employee.Id, "سنوية", new DateTime(2026, 7, 10), new DateTime(2026, 7, 12), null));
        var validLeave = await service.CreateLeaveRequestAsync(new CreateEmployeeLeaveRequest(employee.Id, "سنوية", new DateTime(2026, 7, 13), new DateTime(2026, 7, 14), null));

        var insufficient = await service.DecideLeaveRequestAsync(tooLongLeave.Value.Id, new DecideEmployeeLeaveRequest(true, "موافق"));
        var approved = await service.DecideLeaveRequestAsync(validLeave.Value.Id, new DecideEmployeeLeaveRequest(true, "موافق"));
        var doubleDecision = await service.DecideLeaveRequestAsync(validLeave.Value.Id, new DecideEmployeeLeaveRequest(true, "موافق مرة أخرى"));
        var balances = await service.GetLeaveBalancesAsync(employee.Id, 2026);
        var leaves = (await service.GetLeaveRequestsAsync(employee.Id)).Value.ToList();

        Assert.False(insufficient.IsSuccess);
        Assert.True(approved.IsSuccess);
        Assert.False(doubleDecision.IsSuccess);
        Assert.Equal(2, Assert.Single(balances.Value).UsedDays);
        Assert.Contains(leaves, x => x.Id == tooLongLeave.Value.Id && x.Status == "Pending");
        Assert.Contains(leaves, x => x.Id == validLeave.Value.Id && x.Status == "Approved");
    }

    [Fact]
    public async Task IssueCardAndGeneratePayrollPreview_CreatesOperationalRecords()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        employee.BasicSalary = 5000;
        employee.Allowances = 500;
        await dbcontext.SaveChangesAsync();
        var service = new HumanResourceService(dbcontext);

        var card = await service.IssueCardAsync(new IssueEmployeeCardRequest(employee.Id, "بطاقة موظف", null, new DateTime(2026, 7, 1), null, null));
        var payroll = await service.GeneratePayrollPreviewAsync(new GeneratePayrollPreviewRequest(new DateTime(2026, 7, 1), 100, null));

        Assert.True(card.IsSuccess);
        Assert.StartsWith("EMP-2026-", card.Value.CardNumber);
        Assert.Equal(5400, Assert.Single(payroll.Value).NetSalary);
    }

    [Fact]
    public async Task SafetyAndRecruitmentServices_SaveCoreRecords()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);

        var category = await service.SaveSafetyCategoryAsync(null, new SaveSafetyCategoryRequest("مخاطر كهربائية", "اختبار", true));
        var inspection = await service.SaveSafetyInspectionAsync(null, new SaveSafetyInspectionRequest(category.Value.Id, new DateTime(2026, 7, 8), "المقر", "فحص دوري", "تأمين اللوحات", SafetyRecordStatus.Open));
        var recruitment = await service.SaveRecruitmentRequestAsync(null, new SaveRecruitmentRequest(employee.DepartmentId, employee.JobTitleId, "تعيين أخصائي", 1, "احتياج تشغيلي", null, null, null, null, null, null));

        Assert.True(category.IsSuccess);
        Assert.True(inspection.IsSuccess);
        Assert.True(recruitment.IsSuccess);
        Assert.Equal("Requested", recruitment.Value.Status);
    }

    [Fact]
    public async Task RecruitmentWorkflow_RequiresSequentialCandidateAndInterviewData()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);

        var recruitment = await service.SaveRecruitmentRequestAsync(null, new SaveRecruitmentRequest(employee.DepartmentId, employee.JobTitleId, "تعيين أخصائي", 1, "احتياج تشغيلي", null, null, null, null, null, null));

        var skipped = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Interviewed, null, null, "مقابلة مباشرة"));
        var announced = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Announced, null, null, "تم الإعلان"));
        var receivedWithoutCandidate = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Received, null, null, "وصلت السيرة"));

        await service.SaveRecruitmentRequestAsync(recruitment.Value.Id, new SaveRecruitmentRequest(employee.DepartmentId, employee.JobTitleId, "تعيين أخصائي", 1, "احتياج تشغيلي", "مرشح تجريبي", "0500000000", null, null, null, null));
        var received = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Received, null, null, "استلام المرشح"));
        var completedTooEarly = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Completed, null, null, "اعتماد نهائي"));
        var interviewedWithoutNotes = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Interviewed, null, null, null));
        var interviewed = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Interviewed, null, null, "مقابلة مناسبة"));
        var completed = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Completed, null, null, "اكتمل الطلب"));
        var cancelClosed = await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Cancelled, null, null, "إلغاء بعد الإغلاق"));

        var cancellation = await service.SaveRecruitmentRequestAsync(null, new SaveRecruitmentRequest(employee.DepartmentId, employee.JobTitleId, "تعيين محاسب", 1, "احتياج مالي", null, null, null, null, null, null));
        var cancelWithoutNotes = await service.UpdateRecruitmentStatusAsync(cancellation.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Cancelled, null, null, null));
        var cancelled = await service.UpdateRecruitmentStatusAsync(cancellation.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Cancelled, null, null, "تغير الاحتياج"));

        Assert.False(skipped.IsSuccess);
        Assert.True(announced.IsSuccess);
        Assert.False(receivedWithoutCandidate.IsSuccess);
        Assert.True(received.IsSuccess);
        Assert.False(completedTooEarly.IsSuccess);
        Assert.False(interviewedWithoutNotes.IsSuccess);
        Assert.True(interviewed.IsSuccess);
        Assert.NotNull(interviewed.Value.InterviewAt);
        Assert.True(completed.IsSuccess);
        Assert.Equal("Completed", completed.Value.Status);
        Assert.False(cancelClosed.IsSuccess);
        Assert.False(cancelWithoutNotes.IsSuccess);
        Assert.True(cancelled.IsSuccess);
        Assert.Equal("Cancelled", cancelled.Value.Status);
    }

    [Fact]
    public async Task HumanResourceActivities_TrackRequestHistoryAcrossWorkflows()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var employee = await SeedEmployeeAsync(dbcontext);
        var service = new HumanResourceService(dbcontext);

        var leave = await service.CreateLeaveRequestAsync(new CreateEmployeeLeaveRequest(employee.Id, "سنوية", new DateTime(2026, 7, 10), new DateTime(2026, 7, 12), "سفر"));
        await service.DecideLeaveRequestAsync(leave.Value.Id, new DecideEmployeeLeaveRequest(true, "معتمد"));

        var admin = await service.CreateAdministrativeRequestAsync(new CreateEmployeeAdministrativeRequest(employee.Id, "تعريف", "طلب تعريف", "إصدار تعريف"));
        await service.DecideAdministrativeRequestAsync(admin.Value.Id, new DecideHumanResourceItemRequest(HumanResourceRequestStatus.Approved, "معتمد"));
        await service.DecideAdministrativeRequestAsync(admin.Value.Id, new DecideHumanResourceItemRequest(HumanResourceRequestStatus.Completed, "تم الإصدار"));

        var recruitment = await service.SaveRecruitmentRequestAsync(null, new SaveRecruitmentRequest(employee.DepartmentId, employee.JobTitleId, "تعيين أخصائي", 1, "احتياج", null, null, null, null, null, null));
        await service.UpdateRecruitmentStatusAsync(recruitment.Value.Id, new UpdateRecruitmentStatusRequest(RecruitmentRequestStatus.Announced, null, null, "تم الإعلان"));

        var leaveActivities = await service.GetActivitiesAsync(HumanResourceActivityEntityType.LeaveRequest, leave.Value.Id, null);
        var employeeActivities = await service.GetActivitiesAsync(null, null, employee.Id);
        var recruitmentActivities = await service.GetActivitiesAsync(HumanResourceActivityEntityType.RecruitmentRequest, recruitment.Value.Id, null);

        Assert.Equal(2, leaveActivities.Value.Count());
        Assert.Contains(leaveActivities.Value, x => x.Action == "StatusChanged" && x.FromStatus == "Pending" && x.ToStatus == "Approved");
        Assert.Contains(employeeActivities.Value, x => x.EntityType == "AdministrativeRequest" && x.ToStatus == "Completed");
        Assert.Contains(recruitmentActivities.Value, x => x.Action == "StatusChanged" && x.ToStatus == "Announced");
    }

    private static async Task<EmployeeProfile> SeedEmployeeAsync(Domain.ApplicationDbcontext dbcontext)
    {
        var department = new EmployeeDepartment { NameAr = $"قسم {Guid.NewGuid():N}" };
        var jobTitle = new JobTitle { NameAr = $"مسمى {Guid.NewGuid():N}" };
        var employee = new EmployeeProfile
        {
            EmployeeNumber = $"E-{Guid.NewGuid():N}",
            FullName = "موظف تجريبي",
            Department = department,
            JobTitle = jobTitle,
            HireDate = new DateTime(2026, 1, 1)
        };

        dbcontext.EmployeeProfiles.Add(employee);
        await dbcontext.SaveChangesAsync();
        return employee;
    }
}
