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
    public async Task TaskAndMilestoneProgressWorkflow_RejectsInvalidCompletionProgress()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var incompleteFinishedTask = await service.SaveTaskAsync(null, new SaveProgramProjectTaskRequest(project.Id, "مهمة غير مكتملة", "سعيد", new DateTime(2026, 7, 10), ProgramProjectTaskStatus.Completed, 80, null));
        var runningTask = await service.SaveTaskAsync(null, new SaveProgramProjectTaskRequest(project.Id, "مهمة جارية", "سعيد", new DateTime(2026, 7, 11), ProgramProjectTaskStatus.Running, 80, null));
        var completedTask = await service.SaveTaskAsync(null, new SaveProgramProjectTaskRequest(project.Id, "مهمة مكتملة", "سعيد", new DateTime(2026, 7, 12), ProgramProjectTaskStatus.Completed, 100, null));
        var invalidMilestone = await service.SaveMilestoneAsync(null, new SaveProgramProjectMilestoneRequest(project.Id, "مرحلة زائدة", new DateTime(2026, 7, 1), new DateTime(2026, 7, 3), 101, null));

        Assert.False(incompleteFinishedTask.IsSuccess);
        Assert.True(runningTask.IsSuccess);
        Assert.Equal("Running", runningTask.Value.Status);
        Assert.True(completedTask.IsSuccess);
        Assert.Equal(100, completedTask.Value.ProgressPercent);
        Assert.False(invalidMilestone.IsSuccess);
    }

    [Fact]
    public async Task ProjectLifecycleActions_RecordActivityTrail()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.OnHold, "تعليق مؤقت"));
        await service.SaveTaskAsync(null, new SaveProgramProjectTaskRequest(project.Id, "إغلاق الملفات", "مدير المشروع", new DateTime(2026, 8, 2), ProgramProjectTaskStatus.Completed, 100, "تم الإغلاق"));
        await service.AddFinanceEntryAsync(new AddProgramProjectFinanceEntryRequest(project.Id, ProgramProjectFinanceEntryType.Expense, new DateTime(2026, 8, 3), 750, "مورد", "EXP-1", "مصروف ختامي"));
        await service.AddReportAsync(new AddProgramProjectReportRequest(project.Id, "Final", new DateTime(2026, 8, 4), "تقرير ختامي", null));

        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.Contains(activities, x => x.Type == "Created");
        Assert.Contains(activities, x => x.Type == "StatusChanged" && x.FromStatus == "Active" && x.ToStatus == "OnHold");
        Assert.Contains(activities, x => x.Type == "TaskSaved" && x.Title.Contains("إغلاق الملفات"));
        Assert.Contains(activities, x => x.Type == "FinanceEntryAdded" && x.Amount == 750);
        Assert.Contains(activities, x => x.Type == "ReportAdded" && x.Reference is null);
    }

    [Fact]
    public async Task ProjectStatusWorkflow_RequiresCloseWorkflowAndNotesForClosure()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var directCompleted = await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.Completed, "اكتمال مباشر"));
        var deleteWithoutNotes = await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.Deleted, null));
        var onHold = await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.OnHold, "تعليق مؤقت"));
        var reopened = await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.Active, "استئناف"));
        var cancelled = await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.Cancelled, "إلغاء المشروع"));
        var reopenCancelled = await service.UpdateProjectStatusAsync(project.Id, new UpdateProgramProjectStatusRequest(ProgramProjectStatus.Active, "إعادة فتح"));
        var closeCancelled = await service.CloseProjectAsync(project.Id, new CloseProgramProjectRequest(new DateTime(2026, 8, 1), "تقرير بعد الإلغاء", null, null));

        Assert.False(directCompleted.IsSuccess);
        Assert.False(deleteWithoutNotes.IsSuccess);
        Assert.True(onHold.IsSuccess);
        Assert.Equal("OnHold", onHold.Value.Status);
        Assert.True(reopened.IsSuccess);
        Assert.Equal("Active", reopened.Value.Status);
        Assert.True(cancelled.IsSuccess);
        Assert.Equal("Cancelled", cancelled.Value.Status);
        Assert.False(reopenCancelled.IsSuccess);
        Assert.False(closeCancelled.IsSuccess);
    }

    [Fact]
    public async Task CloseProjectAsync_AddsFinalReportAndCompletesProject()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var close = await service.CloseProjectAsync(project.Id, new CloseProgramProjectRequest(new DateTime(2026, 8, 1), "اكتمل المشروع ورفعت المخرجات.", "final.pdf", "إغلاق إداري"));
        var duplicate = await service.CloseProjectAsync(project.Id, new CloseProgramProjectRequest(new DateTime(2026, 8, 2), "محاولة إغلاق ثانية", null, null));
        var reports = await service.GetReportsAsync(project.Id, "Final");
        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.True(close.IsSuccess);
        Assert.Equal("Completed", close.Value.Status);
        Assert.Equal(new DateTime(2026, 8, 1), close.Value.EndsAt);
        Assert.True(duplicate.IsFailure);
        var finalReport = Assert.Single(reports.Value);
        Assert.Equal("اكتمل المشروع ورفعت المخرجات.", finalReport.Summary);
        Assert.Equal("final.pdf", finalReport.FilePath);
        Assert.Contains(activities, x => x.Type == "ReportAdded" && x.Title == "تقرير ختامي");
        Assert.Contains(activities, x => x.Type == "StatusChanged" && x.FromStatus == "Active" && x.ToStatus == "Completed");
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
    public async Task SupplierProposalWorkflow_ApprovesAndConvertsToContract()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);
        var supplier = await service.SaveSupplierAsync(null, new SaveProgramSupplierRequest("مورد تجهيزات", "خالد", "0500000002", null, "الرياض", ProgramSupplierStatus.Active, null));

        var proposal = await service.SaveSupplierProposalAsync(null, new SaveProgramSupplierProposalRequest(
            project.Id,
            supplier.Value.Id,
            null,
            "تجهيز قاعة التدريب",
            "توريد وتجهيز القاعة",
            4200,
            new DateTime(2026, 7, 15),
            new DateTime(2026, 8, 15),
            ProgramSupplierProposalStatus.Submitted,
            null));
        var approved = await service.DecideSupplierProposalAsync(proposal.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Approved, "أفضل عرض"));
        var converted = await service.ConvertSupplierProposalToContractAsync(proposal.Value.Id, new ConvertProgramSupplierProposalRequest(null, new DateTime(2026, 7, 16), null, "تحويل للعقد"));
        var contracts = await service.GetContractsAsync(project.Id);
        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.True(proposal.IsSuccess);
        Assert.StartsWith("PROP-2026-", proposal.Value.ProposalNumber);
        Assert.Equal("Approved", approved.Value.Status);
        Assert.Equal("Converted", converted.Value.Status);
        Assert.NotNull(converted.Value.ConvertedContractId);
        Assert.Equal(4200, Assert.Single(contracts.Value).Amount);
        Assert.Contains(activities, x => x.Type == "SupplierProposalConverted" && x.Amount == 4200);
        Assert.Contains(activities, x => x.Type == "ContractSaved" && x.Reference is not null && x.Reference.StartsWith("CTR-2026-"));
    }

    [Fact]
    public async Task ContractPaymentWorkflow_CreatesFinanceExpenseAndPreventsOverpayment()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);
        var supplier = await service.SaveSupplierAsync(null, new SaveProgramSupplierRequest("مورد خدمات", "خالد", "0500000003", null, "الرياض", ProgramSupplierStatus.Active, null));
        var contract = await service.SaveContractAsync(null, new SaveProgramProjectContractRequest(project.Id, supplier.Value.Id, null, "تنفيذ خدمة", 4200, new DateTime(2026, 7, 16), null, "عقد خدمات"));

        var firstPayment = await service.RecordContractPaymentAsync(contract.Value.Id, new RecordProgramContractPaymentRequest(3000, new DateTime(2026, 7, 20), "PAY-1", "دفعة أولى"));
        var overpayment = await service.RecordContractPaymentAsync(contract.Value.Id, new RecordProgramContractPaymentRequest(1300, new DateTime(2026, 7, 21), "PAY-2", null));
        var finalPayment = await service.RecordContractPaymentAsync(contract.Value.Id, new RecordProgramContractPaymentRequest(1200, new DateTime(2026, 7, 22), "PAY-3", "دفعة ختامية"));
        var extraPayment = await service.RecordContractPaymentAsync(contract.Value.Id, new RecordProgramContractPaymentRequest(1, new DateTime(2026, 7, 23), "PAY-4", null));
        var finance = (await service.GetFinanceEntriesAsync(project.Id, ProgramProjectFinanceEntryType.Expense)).Value.ToList();
        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.True(contract.IsSuccess);
        Assert.True(firstPayment.IsSuccess);
        Assert.Equal(3000, firstPayment.Value.PaidAmount);
        Assert.Equal(1200, firstPayment.Value.RemainingAmount);
        Assert.False(overpayment.IsSuccess);
        Assert.True(finalPayment.IsSuccess);
        Assert.Equal(4200, finalPayment.Value.PaidAmount);
        Assert.Equal(0, finalPayment.Value.RemainingAmount);
        Assert.False(extraPayment.IsSuccess);
        Assert.Equal(2, finance.Count(x => x.ReferenceNumber == contract.Value.ContractNumber));
        Assert.Equal(4200, finance.Where(x => x.ReferenceNumber == contract.Value.ContractNumber).Sum(x => x.Amount));
        Assert.Contains(finance, x => x.SourceOrPayee == "مورد خدمات" && x.Notes != null && x.Notes.Contains("PAY-1"));
        Assert.Contains(activities, x => x.Type == "FinanceEntryAdded" && x.Title.Contains("سداد عقد") && x.Reference == contract.Value.ContractNumber);
    }

    [Fact]
    public async Task SupplierProposalDecisionWorkflow_RejectsExpiredInvalidAndClosedDecisions()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);
        var supplier = await service.SaveSupplierAsync(null, new SaveProgramSupplierRequest("مورد قرارات", "نواف", "0500000004", null, "الرياض", ProgramSupplierStatus.Active, null));
        var today = DateTime.UtcNow.AddHours(3).Date;

        var draft = await service.SaveSupplierProposalAsync(null, new SaveProgramSupplierProposalRequest(project.Id, supplier.Value.Id, null, "عرض مسودة", null, 1000, today, today.AddDays(10), ProgramSupplierProposalStatus.Draft, null));
        var expired = await service.SaveSupplierProposalAsync(null, new SaveProgramSupplierProposalRequest(project.Id, supplier.Value.Id, null, "عرض منتهي", null, 1000, today.AddDays(-5), today.AddDays(-1), ProgramSupplierProposalStatus.Submitted, null));
        var review = await service.SaveSupplierProposalAsync(null, new SaveProgramSupplierProposalRequest(project.Id, supplier.Value.Id, null, "عرض للمراجعة", null, 2000, today, today.AddDays(10), ProgramSupplierProposalStatus.Submitted, null));
        var convertible = await service.SaveSupplierProposalAsync(null, new SaveProgramSupplierProposalRequest(project.Id, supplier.Value.Id, null, "عرض قابل للتحويل", null, 3000, today, today.AddDays(10), ProgramSupplierProposalStatus.Submitted, null));

        var approveDraft = await service.DecideSupplierProposalAsync(draft.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Approved, "اعتماد مباشر"));
        var approveExpired = await service.DecideSupplierProposalAsync(expired.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Approved, "اعتماد عرض منتهي"));
        var underReview = await service.DecideSupplierProposalAsync(review.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.UnderReview, null));
        var rejectWithoutNotes = await service.DecideSupplierProposalAsync(review.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Rejected, null));
        var rejected = await service.DecideSupplierProposalAsync(review.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Rejected, "غير مناسب فنياً"));
        var approveRejected = await service.DecideSupplierProposalAsync(review.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Approved, "تراجع عن الرفض"));
        var approved = await service.DecideSupplierProposalAsync(convertible.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Approved, "عرض مناسب"));
        await service.ConvertSupplierProposalToContractAsync(convertible.Value.Id, new ConvertProgramSupplierProposalRequest(null, today, null, "تحويل"));
        var rejectConverted = await service.DecideSupplierProposalAsync(convertible.Value.Id, new DecideProgramSupplierProposalRequest(ProgramSupplierProposalStatus.Rejected, "إلغاء بعد التحويل"));

        Assert.False(approveDraft.IsSuccess);
        Assert.False(approveExpired.IsSuccess);
        Assert.True(underReview.IsSuccess);
        Assert.Equal("UnderReview", underReview.Value.Status);
        Assert.False(rejectWithoutNotes.IsSuccess);
        Assert.True(rejected.IsSuccess);
        Assert.Equal("Rejected", rejected.Value.Status);
        Assert.False(approveRejected.IsSuccess);
        Assert.True(approved.IsSuccess);
        Assert.False(rejectConverted.IsSuccess);
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
    public async Task ConvertIdeaToProjectAsync_CreatesProjectAfterApprovalOnce()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var idea = await service.SaveIdeaAsync(null, new SaveProgramIdeaRequest("برنامج تأهيل مهني", "فريق البرامج", "تأهيل مستفيدين لسوق العمل", "فرصة تسويقية", 32000, ProgramIdeaStatus.Pending));
        var approval = await service.SaveApprovalAsync(null, new SaveProgramApprovalRequest(idea.Value.Id, "Idea", "اعتماد برنامج التأهيل"));

        var beforeApproval = await service.ConvertIdeaToProjectAsync(idea.Value.Id, new ConvertProgramIdeaToProjectRequest("Program", "مدير البرامج", new DateTime(2026, 9, 1), new DateTime(2026, 10, 1), ProgramProjectStatus.Planning, 40, "تحويل قبل الاعتماد"));
        await service.DecideApprovalAsync(approval.Value.Id, new DecideProgramApprovalRequest(ProgramApprovalStatus.Approved, "معتمد للتنفيذ"));
        var conversion = await service.ConvertIdeaToProjectAsync(idea.Value.Id, new ConvertProgramIdeaToProjectRequest("Program", "مدير البرامج", new DateTime(2026, 9, 1), new DateTime(2026, 10, 1), ProgramProjectStatus.Planning, 40, "تحويل للتنفيذ"));
        var duplicate = await service.ConvertIdeaToProjectAsync(idea.Value.Id, new ConvertProgramIdeaToProjectRequest("Program", null, null, null, ProgramProjectStatus.Planning, 0, null));
        var completedIdeas = await service.GetIdeasAsync(ProgramIdeaStatus.Completed);
        var activities = (await service.GetProjectActivitiesAsync(conversion.Value.Id)).Value.ToList();

        Assert.True(beforeApproval.IsFailure);
        Assert.True(conversion.IsSuccess);
        Assert.StartsWith("PRJ-2026-", conversion.Value.ProjectCode);
        Assert.Equal("برنامج تأهيل مهني", conversion.Value.Name);
        Assert.Equal(32000, conversion.Value.Budget);
        Assert.True(duplicate.IsFailure);
        var convertedIdea = Assert.Single(completedIdeas.Value);
        Assert.Equal(conversion.Value.Id, convertedIdea.ConvertedProjectId);
        Assert.Equal(conversion.Value.ProjectCode, convertedIdea.ConvertedProjectCode);
        Assert.Contains(activities, x => x.Type == "Created" && x.Reference == $"IDEA-{idea.Value.Id}");
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
    public async Task SurveySubmissionWorkflow_RequiresActiveSurveyValidJsonAndUniqueRespondent()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var invalidQuestions = await service.SaveSurveyAsync(null, new SaveProgramSurveyRequest(project.Id, "استبيان غير صالح", "{\"question\":1}", ProgramSurveyStatus.Active, null));
        var draft = await service.SaveSurveyAsync(null, new SaveProgramSurveyRequest(project.Id, "استبيان مسودة", "[\"التقييم\"]", ProgramSurveyStatus.Draft, null));
        var active = await service.SaveSurveyAsync(null, new SaveProgramSurveyRequest(project.Id, "استبيان نشط", "[\"التقييم\"]", ProgramSurveyStatus.Active, null));

        var draftSubmission = await service.AddSurveySubmissionAsync(new AddProgramSurveySubmissionRequest(draft.Value.Id, "مشارك مسودة", "{\"rating\":4}", new DateTime(2026, 7, 10)));
        var invalidAnswers = await service.AddSurveySubmissionAsync(new AddProgramSurveySubmissionRequest(active.Value.Id, "مشارك أول", "[5]", new DateTime(2026, 7, 10)));
        var submission = await service.AddSurveySubmissionAsync(new AddProgramSurveySubmissionRequest(active.Value.Id, "مشارك أول", "{\"rating\":5}", new DateTime(2026, 7, 10)));
        var duplicate = await service.AddSurveySubmissionAsync(new AddProgramSurveySubmissionRequest(active.Value.Id, "مشارك أول", "{\"rating\":3}", new DateTime(2026, 7, 11)));
        await service.SaveSurveyAsync(active.Value.Id, new SaveProgramSurveyRequest(project.Id, "استبيان نشط", "[\"التقييم\"]", ProgramSurveyStatus.Closed, "إغلاق"));
        var closedSubmission = await service.AddSurveySubmissionAsync(new AddProgramSurveySubmissionRequest(active.Value.Id, "مشارك ثاني", "{\"rating\":4}", new DateTime(2026, 7, 12)));
        var submissions = await service.GetSurveySubmissionsAsync(active.Value.Id);
        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.False(invalidQuestions.IsSuccess);
        Assert.True(draft.IsSuccess);
        Assert.True(active.IsSuccess);
        Assert.False(draftSubmission.IsSuccess);
        Assert.False(invalidAnswers.IsSuccess);
        Assert.True(submission.IsSuccess);
        Assert.False(duplicate.IsSuccess);
        Assert.False(closedSubmission.IsSuccess);
        var savedSubmission = Assert.Single(submissions.Value);
        Assert.Equal("مشارك أول", savedSubmission.RespondentName);
        Assert.Contains(activities, x => x.Type == "SurveySaved" && x.Title.Contains("إجابة استبيان"));
    }

    [Fact]
    public async Task AttendanceAndRegistrationCertificate_WorkAsConnectedWorkflow()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);

        var registration = await service.SaveRegistrationAsync(null, new SaveProgramRegistrationRequest(project.Id, "مشارك شهادة", "0500000011", null, "BEN-CERT", new DateTime(2026, 7, 8), null));
        var earlyCertificate = await service.IssueCertificateFromRegistrationAsync(registration.Value.Id, new IssueProgramCertificateFromRegistrationRequest(null, null, new DateTime(2026, 7, 8), null));
        await service.DecideRegistrationAsync(registration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Approved, "مقبول"));
        var session = await service.SaveSessionAsync(null, new SaveProgramSessionRequest(project.Id, "جلسة الحضور", new DateTime(2026, 7, 9, 9, 0, 0), new DateTime(2026, 7, 9, 11, 0, 0), "المقر", null));

        var attendance = await service.SaveAttendanceAsync(null, new SaveProgramSessionAttendanceRequest(session.Value.Id, "مشارك شهادة", "BEN-CERT", ProgramAttendanceStatus.Present, "حضر البرنامج"));
        var attendedRegistrations = await service.GetRegistrationsAsync(project.Id, ProgramRegistrationStatus.Attended);
        var template = await service.SaveCertificateTemplateAsync(null, new SaveProgramCertificateTemplateRequest(project.Id, "قالب إتمام", "شهادة إتمام", true));
        var certificate = await service.IssueCertificateFromRegistrationAsync(registration.Value.Id, new IssueProgramCertificateFromRegistrationRequest(template.Value.Id, null, new DateTime(2026, 7, 10), "إصدار بعد الحضور"));
        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.True(earlyCertificate.IsFailure);
        Assert.True(attendance.IsSuccess);
        var attendedRegistration = Assert.Single(attendedRegistrations.Value);
        Assert.Equal(registration.Value.Id, attendedRegistration.Id);
        Assert.Equal("Attended", attendedRegistration.Status);
        Assert.True(certificate.IsSuccess);
        Assert.Equal("مشارك شهادة", certificate.Value.RecipientName);
        Assert.Equal(template.Value.Id, certificate.Value.ProgramCertificateTemplateId);
        Assert.Contains(activities, x => x.Type == "RegistrationDecided" && x.FromStatus == "Approved" && x.ToStatus == "Attended");
        Assert.Contains(activities, x => x.Type == "CertificateIssued" && x.Reference == certificate.Value.CertificateNumber);
    }

    [Fact]
    public async Task RegistrationApprovalAndAttendance_RespectProgramCapacity()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await service.SaveProjectAsync(null, new SaveProgramProjectRequest(
            "برنامج محدود المقاعد",
            null,
            "Program",
            null,
            null,
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 31),
            ProgramProjectStatus.Active,
            10000,
            1,
            null));
        var first = await service.SaveRegistrationAsync(null, new SaveProgramRegistrationRequest(project.Value.Id, "المشارك الأول", null, null, "REG-1", null, null));
        var second = await service.SaveRegistrationAsync(null, new SaveProgramRegistrationRequest(project.Value.Id, "المشارك الثاني", null, null, "REG-2", null, null));
        var session = await service.SaveSessionAsync(null, new SaveProgramSessionRequest(project.Value.Id, "اللقاء", new DateTime(2026, 7, 2, 9, 0, 0), new DateTime(2026, 7, 2, 11, 0, 0), null, null));

        var approveFirst = await service.DecideRegistrationAsync(first.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Approved, "مقبول"));
        var approveSecond = await service.DecideRegistrationAsync(second.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Approved, "مقبول"));
        var secondAttendance = await service.SaveAttendanceAsync(null, new SaveProgramSessionAttendanceRequest(session.Value.Id, "المشارك الثاني", "REG-2", ProgramAttendanceStatus.Present, null));
        var firstAttendance = await service.SaveAttendanceAsync(null, new SaveProgramSessionAttendanceRequest(session.Value.Id, "المشارك الأول", "REG-1", ProgramAttendanceStatus.Present, null));
        var attended = await service.GetRegistrationsAsync(project.Value.Id, ProgramRegistrationStatus.Attended);

        Assert.True(approveFirst.IsSuccess);
        Assert.True(approveSecond.IsFailure);
        Assert.True(secondAttendance.IsSuccess);
        Assert.True(firstAttendance.IsSuccess);
        var attendedRegistration = Assert.Single(attended.Value);
        Assert.Equal(first.Value.Id, attendedRegistration.Id);
    }

    [Fact]
    public async Task RegistrationDecisionWorkflow_RequiresPendingApprovalBeforeAttendance()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);
        var registration = await service.SaveRegistrationAsync(null, new SaveProgramRegistrationRequest(project.Id, "مشارك بوابة", "0500000099", null, "REG-GATE", new DateTime(2026, 7, 8), null));
        var session = await service.SaveSessionAsync(null, new SaveProgramSessionRequest(project.Id, "جلسة البوابة", new DateTime(2026, 7, 9, 9, 0, 0), new DateTime(2026, 7, 9, 11, 0, 0), "المقر", null));

        var earlyAttendance = await service.SaveAttendanceAsync(null, new SaveProgramSessionAttendanceRequest(session.Value.Id, "مشارك بوابة", "REG-GATE", ProgramAttendanceStatus.Present, "حضور قبل الاعتماد"));
        var directAttended = await service.DecideRegistrationAsync(registration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Attended, "تحضير يدوي"));
        var rejectWithoutNotes = await service.DecideRegistrationAsync(registration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Rejected, null));
        var approved = await service.DecideRegistrationAsync(registration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Approved, "مقبول"));
        var doubleDecision = await service.DecideRegistrationAsync(registration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Rejected, "تراجع"));
        var approvedAttendance = await service.SaveAttendanceAsync(null, new SaveProgramSessionAttendanceRequest(session.Value.Id, "مشارك بوابة", "REG-GATE", ProgramAttendanceStatus.Present, "حضور بعد الاعتماد"));
        var rejectedRegistration = await service.SaveRegistrationAsync(null, new SaveProgramRegistrationRequest(project.Id, "مشارك مرفوض", null, null, "REG-REJECT", new DateTime(2026, 7, 8), null));
        var rejected = await service.DecideRegistrationAsync(rejectedRegistration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Rejected, "غير مطابق للشروط"));
        var approveRejected = await service.DecideRegistrationAsync(rejectedRegistration.Value.Id, new DecideProgramRegistrationRequest(ProgramRegistrationStatus.Approved, "تراجع"));
        var attended = await service.GetRegistrationsAsync(project.Id, ProgramRegistrationStatus.Attended);
        var rejectedList = await service.GetRegistrationsAsync(project.Id, ProgramRegistrationStatus.Rejected);

        Assert.True(earlyAttendance.IsSuccess);
        Assert.False(directAttended.IsSuccess);
        Assert.False(rejectWithoutNotes.IsSuccess);
        Assert.True(approved.IsSuccess);
        Assert.Equal("Approved", approved.Value.Status);
        Assert.False(doubleDecision.IsSuccess);
        Assert.True(approvedAttendance.IsSuccess);
        var attendedRegistration = Assert.Single(attended.Value);
        Assert.Equal(registration.Value.Id, attendedRegistration.Id);
        Assert.True(rejected.IsSuccess);
        Assert.Equal("Rejected", rejected.Value.Status);
        Assert.False(approveRejected.IsSuccess);
        Assert.Contains(rejectedList.Value, x => x.Id == rejectedRegistration.Value.Id);
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
    public async Task GenerateQualificationInstallmentsAsync_CreatesBalancedScheduleAndActivatesCase()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ProgramsProjectsService(dbcontext);
        var project = await SeedProjectAsync(service);
        var draft = await service.SaveQualificationCaseAsync(null, new SaveProgramQualificationCaseRequest(project.Id, "مستفيد جدول", "تمويل مشروع صغير", "مناسب", ProgramQualificationCaseStatus.Required, 1000, 3, null));

        var beforeApproval = await service.GenerateQualificationInstallmentsAsync(draft.Value.Id, new GenerateQualificationInstallmentsRequest(new DateTime(2026, 8, 1), 1, "جدولة"));
        await service.UpdateQualificationCaseStatusAsync(draft.Value.Id, new UpdateProgramQualificationCaseStatusRequest(ProgramQualificationCaseStatus.Approved, "معتمد", null));
        var generated = await service.GenerateQualificationInstallmentsAsync(draft.Value.Id, new GenerateQualificationInstallmentsRequest(new DateTime(2026, 8, 1), 1, "جدولة"));
        var duplicate = await service.GenerateQualificationInstallmentsAsync(draft.Value.Id, new GenerateQualificationInstallmentsRequest(new DateTime(2026, 8, 1), 1, null));
        var activeCases = await service.GetQualificationCasesAsync(ProgramQualificationCaseStatus.Active);
        var activities = (await service.GetProjectActivitiesAsync(project.Id)).Value.ToList();

        Assert.True(beforeApproval.IsFailure);
        Assert.True(generated.IsSuccess);
        var installments = generated.Value.ToList();
        Assert.Equal(3, installments.Count);
        Assert.Equal(1000, installments.Sum(x => x.Amount));
        Assert.Equal(new DateTime(2026, 8, 1), installments[0].DueDate);
        Assert.Equal(new DateTime(2026, 9, 1), installments[1].DueDate);
        Assert.Equal(new DateTime(2026, 10, 1), installments[2].DueDate);
        Assert.Equal(333.34m, installments[2].Amount);
        Assert.True(duplicate.IsFailure);
        var activeCase = Assert.Single(activeCases.Value);
        Assert.Equal(draft.Value.Id, activeCase.Id);
        Assert.Contains(activities, x => x.Type == "FinanceEntryAdded" && x.ToStatus == "Active" && x.Amount == 1000);
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
