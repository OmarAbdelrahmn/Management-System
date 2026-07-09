using Application.Contracts.ProgramsProjects;
using Application.Service.ProgramsProjects;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class ProgramsProjectsServiceTests
{
    [Fact]
    public async Task SaveProjectAsync_GeneratesProjectCode()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);

        var result = await service.SaveProjectAsync(null, new SaveProgramProjectRequest(
            "برنامج تدريبي",
            null,
            "Program",
            "برنامج تجريبي",
            "مدير البرنامج",
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 31),
            ProgramProjectStatus.Planning,
            10000,
            50,
            null));

        Assert.True(result.IsSuccess);
        Assert.StartsWith("PRJ-2026-", result.Value.ProjectCode);
        Assert.Equal("Planning", result.Value.Status);
    }

    [Fact]
    public async Task PlanningAndFinanceEntries_UpdateProjectBalance()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var task = await service.SaveTaskAsync(null, new SaveProgramProjectTaskRequest(project.Id, "تهيئة الخطة", "سعيد", new DateTime(2026, 7, 10), ProgramProjectTaskStatus.Completed, 100, null));
        var milestone = await service.SaveMilestoneAsync(null, new SaveProgramProjectMilestoneRequest(project.Id, "إطلاق البرنامج", new DateTime(2026, 7, 1), new DateTime(2026, 7, 3), 60, null));
        var income = await service.AddFinanceEntryAsync(new AddProgramProjectFinanceEntryRequest(project.Id, ProgramProjectFinanceEntryType.Income, new DateTime(2026, 7, 2), 5000, "داعم", "R-1", null));
        var expense = await service.AddFinanceEntryAsync(new AddProgramProjectFinanceEntryRequest(project.Id, ProgramProjectFinanceEntryType.Expense, new DateTime(2026, 7, 3), 1200, "مورد", "E-1", null));

        var projects = await service.SearchProjectsAsync(new ProgramProjectSearchRequest(null, null, null));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(task.IsSuccess);
        Assert.True(milestone.IsSuccess);
        Assert.True(income.IsSuccess);
        Assert.True(expense.IsSuccess);
        var savedProject = Assert.Single(projects.Value);
        Assert.Equal(3800, savedProject.Balance);
        Assert.Equal(1, savedProject.CompletedTasksCount);
        Assert.Equal(5000, dashboard.Value.TotalIncome);
        Assert.Equal(1200, dashboard.Value.TotalExpenses);
    }

    [Fact]
    public async Task ProjectLifecycleActions_RecordActivityTrail()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.Completed, "اكتمل التنفيذ"));
        await service.SaveTaskAsync(null, new SaveProgramProjectTaskRequest(project.Id, "إغلاق الملفات", "مدير المشروع", new DateTime(2026, 8, 2), ProgramProjectTaskStatus.Completed, 100, "تم الإغلاق"));
        await service.AddFinanceEntryAsync(new AddProgramProjectFinanceEntryRequest(project.Id, ProgramProjectFinanceEntryType.Expense, new DateTime(2026, 8, 3), 750, "مورد", "EXP-1", "مصروف ختامي"));
        await service.AddReportAsync(new AddProgramProjectReportRequest(project.Id, "Final", new DateTime(2026, 8, 4), "تقرير ختامي", null));

        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.Contains(activities, x => x.Type == "Created");
        Assert.Contains(activities, x => x.Type == "StatusChanged" && x.FromStatus == "Active" && x.ToStatus == "Completed");
        Assert.Contains(activities, x => x.Type == "TaskSaved" && x.Title.Contains("إغلاق الملفات"));
        Assert.Contains(activities, x => x.Type == "FinanceEntryAdded" && x.Amount == 750);
        Assert.Contains(activities, x => x.Type == "ReportAdded" && x.Reference is null);
    }

    [Fact]
    public async Task SaveSupplierAsync_RejectsDuplicateSupplierName()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);

        var first = await service.SaveSupplierAsync(null, new SaveProgramSupplierRequest("مورد التدريب", "أحمد", "0500000000", "supplier@example.com", "الرياض", ProgramSupplierStatus.Active, null));
        var duplicate = await service.SaveSupplierAsync(null, new SaveProgramSupplierRequest("مورد التدريب", null, null, null, null, ProgramSupplierStatus.Active, null));

        Assert.True(first.IsSuccess);
        Assert.True(duplicate.IsFailure);
    }

    [Fact]
    public async Task DecideApprovalAsync_UpdatesLinkedIdeaStatus()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var idea = await service.SaveIdeaAsync(null, new SaveProgramIdeaRequest("برنامج صيفي", "فريق البرامج", "تنفيذ برنامج صيفي", "قابل للتسويق", 25000, ProgramIdeaStatus.Pending));
        var approval = await service.SaveApprovalAsync(null, new SaveProgramApprovalRequest(idea.Value.Id, "Idea", "اعتماد البرنامج الصيفي"));

        var decision = await service.DecideApprovalAsync(approval.Value.Id, new DecideProgramApprovalRequest(ProgramApprovalStatus.Approved, "موافق"));
        var ideas = await service.GetIdeasAsync(ProgramIdeaStatus.Approved);

        Assert.True(decision.IsSuccess);
        var approvedIdea = Assert.Single(ideas.Value);
        Assert.Equal(idea.Value.Id, approvedIdea.Id);
        Assert.Equal("موافق", approvedIdea.DecisionNotes);
    }

    [Fact]
    public async Task ProgramSpecialtyFlows_SaveRegistrationAttendanceSurveyAndCertificate()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var registration = await service.SaveRegistrationAsync(null, new SaveProgramRegistrationRequest(project.Id, "مشارك أول", "0500000000", null, "BEN-1", new DateTime(2026, 7, 8), null));
        var decision = await service.DecideRegistrationAsync(registration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Approved, "مقبول"));
        var session = await service.SaveSessionAsync(null, new SaveProgramSessionRequest(project.Id, "اليوم الأول", new DateTime(2026, 7, 9, 9, 0, 0), new DateTime(2026, 7, 9, 11, 0, 0), "المقر", null));
        var attendance = await service.SaveAttendanceAsync(null, new SaveProgramSessionAttendanceRequest(session.Value.Id, "مشارك أول", "BEN-1", ProgramAttendanceStatus.Present, null));
        var survey = await service.SaveSurveyAsync(null, new SaveProgramSurveyRequest(project.Id, "تقييم البرنامج", "[\"التقييم\"]", ProgramSurveyStatus.Active, null));
        var submission = await service.AddSurveySubmissionAsync(new AddProgramSurveySubmissionRequest(survey.Value.Id, "مشارك أول", "{\"rating\":5}", new DateTime(2026, 7, 10)));
        var template = await service.SaveCertificateTemplateAsync(null, new SaveProgramCertificateTemplateRequest(project.Id, "قالب حضور", "شهادة حضور", true));
        var certificate = await service.IssueCertificateAsync(new IssueProgramCertificateRequest(project.Id, template.Value.Id, null, "مشارك أول", new DateTime(2026, 7, 11), null));

        Assert.True(decision.IsSuccess);
        Assert.Equal("Approved", decision.Value.Status);
        Assert.True(attendance.IsSuccess);
        Assert.Equal("Present", attendance.Value.Status);
        Assert.True(submission.IsSuccess);
        Assert.StartsWith("CERT-2026-", certificate.Value.CertificateNumber);
    }

    [Fact]
    public async Task QualificationInstallmentPayment_UpdatesCaseBalanceAndStatus()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);
        var qualification = await service.SaveQualificationCaseAsync(null, new SaveProgramQualificationCaseRequest(project.Id, "مستفيد تأهيل", "بحاجة إلى مشروع صغير", "مناسب", ProgramQualificationCaseStatus.Approved, 3000, 1, null));
        var installment = await service.SaveQualificationInstallmentAsync(null, new SaveProgramQualificationInstallmentRequest(qualification.Value.Id, new DateTime(2026, 8, 1), 3000, null));

        var payment = await service.RecordQualificationInstallmentPaymentAsync(installment.Value.Id, new RecordQualificationInstallmentPaymentRequest(3000, new DateTime(2026, 8, 1), "سداد كامل"));
        var cases = await service.GetQualificationCasesAsync(ProgramQualificationCaseStatus.Paid);

        Assert.True(payment.IsSuccess);
        Assert.Equal("Paid", payment.Value.Status);
        var paidCase = Assert.Single(cases.Value);
        Assert.Equal(0, paidCase.RemainingAmount);
    }

    [Fact]
    public async Task PublishProjectAndRegistrationForm_SaveProgramMetadata()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var form = await service.SaveRegistrationFormAsync(project.Id, new SaveProgramRegistrationFormRequest("برنامج خاص", "[{\"name\":\"mobile\"}]"));
        var publish = await service.PublishProjectAsync(project.Id, new PublishProgramProjectRequest(true, new DateTime(2026, 7, 12), "نشر للتسجيل"));

        Assert.True(form.IsSuccess);
        Assert.Equal("برنامج خاص", form.Value.SpecialProgramCategory);
        Assert.True(publish.IsSuccess);
        Assert.True(publish.Value.IsPublished);
        Assert.Equal(new DateTime(2026, 7, 12), publish.Value.PublishedAt);
        Assert.Equal("[{\"name\":\"mobile\"}]", publish.Value.RegistrationFormJson);
    }

    private static async Task<ProgramProjectResponse> SeedProjectAsync(ProgramsProjectsService service)
    {
        var project = await service.SaveProjectAsync(null, new SaveProgramProjectRequest(
            "مشروع تمكين",
            null,
            "Project",
            null,
            null,
            new DateTime(2026, 7, 1),
            new DateTime(2026, 8, 1),
            ProgramProjectStatus.Active,
            15000,
            20,
            null));

        return project.Value;
    }
}
