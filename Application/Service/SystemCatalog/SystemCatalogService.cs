using Application.Abstraction;
using Application.Contracts.SystemCatalog;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.SystemCatalog;

public class SystemCatalogService(ApplicationDbcontext dbcontext) : ISystemCatalogService
{
    private static readonly Error PageNotFound = new("SystemCatalog.PageNotFound", "System page was not found.", StatusCodes.Status404NotFound);

    public async Task<Result<SeedSystemCatalogResponse>> SeedFirstPagesAsync(CancellationToken cancellationToken = default)
    {
        var modulesCreated = 0;
        var pagesCreated = 0;
        var pagesUpdated = 0;
        var modules = await dbcontext.SystemModules.Include(x => x.Pages).ToDictionaryAsync(x => x.Key, cancellationToken);

        foreach (var moduleSeed in FirstModules())
        {
            if (!modules.TryGetValue(moduleSeed.Key, out var module))
            {
                module = new SystemModule
                {
                    Key = moduleSeed.Key,
                    NameAr = moduleSeed.NameAr,
                    NameEn = moduleSeed.NameEn,
                    Description = moduleSeed.Description,
                    Priority = moduleSeed.Priority
                };
                dbcontext.SystemModules.Add(module);
                modules[moduleSeed.Key] = module;
                modulesCreated++;
            }
            else
            {
                module.NameAr = moduleSeed.NameAr;
                module.NameEn = moduleSeed.NameEn;
                module.Description = moduleSeed.Description;
                module.Priority = moduleSeed.Priority;
            }

            var existingPages = module.Pages.ToDictionary(x => x.Key);
            foreach (var pageSeed in moduleSeed.Pages)
            {
                if (!existingPages.TryGetValue(pageSeed.Key, out var page))
                {
                    page = new SystemPage { Key = pageSeed.Key, SystemModule = module };
                    dbcontext.SystemPages.Add(page);
                    pagesCreated++;
                }
                else
                {
                    pagesUpdated++;
                }

                page.NameAr = pageSeed.NameAr;
                page.Route = pageSeed.Route;
                page.PermissionKey = pageSeed.PermissionKey;
                page.ServiceName = pageSeed.ServiceName;
                page.ServicePlan = pageSeed.ServicePlan;
                page.UiPlan = pageSeed.UiPlan;
                page.SortOrder = pageSeed.SortOrder;
                if (page.Status == default)
                    page.Status = pageSeed.Status;
            }
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        var totalPages = await dbcontext.SystemPages.CountAsync(cancellationToken);
        return Result.Success(new SeedSystemCatalogResponse(modulesCreated, pagesCreated, pagesUpdated, totalPages));
    }

    public async Task<Result<IEnumerable<SystemModuleResponse>>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        var modules = await dbcontext.SystemModules
            .AsNoTracking()
            .Include(x => x.Pages)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.NameAr)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SystemModuleResponse>>(modules.Select(MapModule));
    }

    public async Task<Result<IEnumerable<SystemPageResponse>>> GetPagesAsync(string? moduleKey = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SystemPages.AsNoTracking().Include(x => x.SystemModule).AsQueryable();
        if (!string.IsNullOrWhiteSpace(moduleKey))
            query = query.Where(x => x.SystemModule != null && x.SystemModule.Key == moduleKey);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SystemPageStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);

        var pages = await query
            .OrderBy(x => x.SystemModule!.Priority)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SystemPageResponse>>(pages.Select(MapPage));
    }

    public async Task<Result<SystemPageResponse>> GetPageAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await dbcontext.SystemPages.AsNoTracking().Include(x => x.SystemModule).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return page is null ? Result.Failure<SystemPageResponse>(PageNotFound) : Result.Success(MapPage(page));
    }

    public async Task<Result> UpdatePageStatusAsync(int id, UpdateSystemPageStatusRequest request, CancellationToken cancellationToken = default)
    {
        var page = await dbcontext.SystemPages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (page is null)
            return Result.Failure(PageNotFound);

        page.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static SystemModuleResponse MapModule(SystemModule module) =>
        new(
            module.Id,
            module.Key,
            module.NameAr,
            module.NameEn,
            module.Description,
            module.Priority,
            module.Pages.Count,
            module.Pages.Count(x => x.Status == SystemPageStatus.Planned),
            module.Pages.Count(x => x.Status == SystemPageStatus.InProgress),
            module.Pages.Count(x => x.Status == SystemPageStatus.Implemented));

    private static SystemPageResponse MapPage(SystemPage page) =>
        new(
            page.Id,
            page.SystemModuleId,
            page.SystemModule?.Key ?? string.Empty,
            page.SystemModule?.NameAr ?? string.Empty,
            page.Key,
            page.NameAr,
            page.Route,
            page.PermissionKey,
            page.ServiceName,
            page.ServicePlan,
            page.UiPlan,
            page.Status.ToString(),
            page.SortOrder);

    private static IReadOnlyList<SystemModuleSeed> FirstModules() =>
    [
        Module("meetings", "إدارة الاجتماعات والمجالس", "Meetings and Boards", "المجالس، الاجتماعات، الدعوات، التصويت، القرارات، المحاضر، والتقويم.", 1,
            Page("meetings.dashboard", "لوحة الاجتماعات", "/meetings/dashboard", "MeetingsDashboardService", "ملخصات ومؤشرات الاجتماعات والدعوات والمحاضر.", "لوحة مؤشرات مع روابط سريعة وفلترة حسب النوع.", 1, SystemPageStatus.InProgress),
            Page("meetings.create", "إنشاء اجتماع", "/meetings/create", "MeetingService", "إنشاء وتعديل الاجتماع مع كل حقول المجلس والتكرار والمرفقات.", "نموذج متعدد الأقسام مع تحقق تواريخ ومهلة قبول.", 2, SystemPageStatus.InProgress),
            Page("meetings.approvals", "اعتماد الاجتماعات", "/meetings/approvals", "MeetingApprovalService", "قائمة إرسال للاعتماد، اعتماد، رفض، وسبب الرفض.", "جدول اعتماد مع إجراءات وصلاحيات.", 3),
            Page("meetings.scheduled", "الاجتماعات المجدولة", "/meetings/scheduled", "MeetingScheduleService", "استعلام وجدولة وتذكيرات ومواعيد قادمة.", "جدول وتبويبات يوم/أسبوع/شهر.", 4),
            Page("meetings.repeat-drafts", "مسودات الاجتماعات المتكررة", "/meetings/repeat-drafts", "MeetingRepeatService", "توليد ومراجعة وتحويل مسودات التكرار لاجتماعات فعلية.", "قائمة مسودات مع زر إنشاء اجتماع.", 5),
            Page("meetings.finish", "إنهاء اجتماع", "/meetings/finish", "MeetingLifecycleService", "إنهاء الاجتماع وقفل التصويت والانتقال للمحضر.", "قائمة اجتماعات جارية مع إجراء إنهاء.", 6),
            Page("meetings.minutes-approval", "اعتماد المحاضر", "/meetings/minutes-approval", "MinuteService", "اعتماد وتوقيع المحاضر ونشر القرارات.", "لوحة الرئيس/الاعتماد مع توقيع رقمي.", 7),
            Page("meetings.archive", "أرشيف المحاضر", "/meetings/archive", "MinuteArchiveService", "أرشفة وقراءة وتحميل PDF.", "جدول Read-Only مع تحميل PDF.", 8, SystemPageStatus.InProgress),
            Page("meetings.calendar", "تقويم الاجتماعات", "/meetings/calendar", "MeetingCalendarService", "عرض الاجتماعات على تقويم.", "تقويم شهر/أسبوع/يوم.", 9),
            Page("meetings.live-voting", "التصويت الحي", "/meetings/live-voting", "VotingService", "فتح/إغلاق/بث التصويت واحتساب الأوزان.", "ثلاثة أعمدة محدثة SignalR.", 10, SystemPageStatus.InProgress)),
        Module("members", "إدارة الأعضاء", "Members", "الأعضاء، العضويات، الرسوم، الإلغاء، البطاقات، والتقارير.", 2,
            Page("members.create", "إضافة عضو", "/members/create", "MemberService", "إنشاء ملف عضو كامل وربطه بحساب مستخدم.", "نموذج بيانات شخصية/عضوية/عنوان/مرفقات.", 1, SystemPageStatus.Implemented),
            Page("members.database", "قاعدة بيانات الأعضاء", "/members/database", "MemberQueryService", "بحث وفرز وتصدير ملفات الأعضاء.", "DataGrid مع فلاتر وتصدير.", 2, SystemPageStatus.Implemented),
            Page("members.update", "تحديث بيانات عضو", "/members/update", "MemberService", "تعديل بيانات العضو مع سجل تغييرات.", "نموذج تعديل مع Audit trail.", 3, SystemPageStatus.Implemented),
            Page("members.cancel", "إلغاء عضوية", "/members/cancel", "MemberLifecycleService", "إلغاء/تعليق عضوية مع سبب وتاريخ.", "قائمة وإجراء إلغاء.", 4, SystemPageStatus.Implemented),
            Page("members.canceled", "الأعضاء الملغاة عضوياتهم", "/members/canceled", "MemberQueryService", "عرض واستعادة العضويات الملغاة.", "جدول Read/Restore.", 5, SystemPageStatus.Implemented),
            Page("members.payments", "سداد الاشتراكات", "/members/payments", "MemberPaymentService", "تسجيل ومراجعة سداد الاشتراكات.", "إيصال/سند وجدول مدفوعات.", 6, SystemPageStatus.Implemented),
            Page("members.due", "المتأخرون عن السداد", "/members/due", "MemberPaymentService", "حساب المتأخرات وتنبيهات السداد.", "قائمة تنبيه وإرسال رسائل.", 7, SystemPageStatus.Implemented),
            Page("members.cards", "بطاقات الأعضاء", "/members/cards", "MemberCardService", "إصدار وتحميل بطاقات PDF.", "قوالب بطاقات وطباعة.", 8, SystemPageStatus.Implemented),
            Page("members.types", "فئات العضويات", "/members/types", "MembershipTypeService", "إدارة أنواع العضويات والرسوم والأصوات.", "CRUD بسيط لفئات العضوية.", 9, SystemPageStatus.Implemented),
            Page("members.reports", "مشاركة تقرير مع الأعضاء", "/members/reports/share", "MemberReportService", "إنشاء ومشاركة تقارير للأعضاء.", "اختيار تقرير ومستلمين وقناة إرسال.", 10, SystemPageStatus.Implemented)),
        Module("tasks", "إدارة المهام والاعتمادات", "Tasks and Approvals", "المهام، الحالات، التحويلات، الاعتماد، وسلاسل الموافقات.", 3,
            Page("tasks.create", "إنشاء مهمة", "/tasks/create", "TaskService", "إنشاء مهمة وربطها بملف/اجتماع/طلب.", "نموذج مهمة مع مكلفين ومرفقات.", 1),
            Page("tasks.mine", "مهامي", "/tasks/mine", "TaskQueryService", "مهام المستخدم الحالية والمكتملة.", "تبويبات جديدة/جارية/متعثرة/منتهية.", 2),
            Page("tasks.manage", "إدارة مهام الموظفين", "/tasks/manage", "TaskManagementService", "تعديل وتحويل وإسناد المهام.", "جدول إدارة مع عمليات.", 3),
            Page("tasks.complete", "إنجاز مهمة", "/tasks/complete", "TaskLifecycleService", "إنهاء مهمة وتسجيل نسبة الإنجاز.", "نموذج إنجاز ومرفقات إثبات.", 4),
            Page("tasks.redirect", "تحويل مهمة متعثرة", "/tasks/redirect", "TaskLifecycleService", "تحويل مهمة لموظف آخر مع سبب.", "إجراء تحويل من قائمة المتعثرة.", 5),
            Page("tasks.delete", "حذف مهمة", "/tasks/delete", "TaskLifecycleService", "حذف منطقي مع سبب.", "قائمة حذف واستعادة.", 6),
            Page("tasks.restore", "استعادة مهمة", "/tasks/restore", "TaskLifecycleService", "استعادة مهمة محذوفة.", "جدول محذوفات.", 7),
            Page("tasks.database", "قاعدة بيانات المهام", "/tasks/database", "TaskQueryService", "كل المهام مع فلاتر وتصدير.", "DataGrid شامل.", 8),
            Page("approvals.routes", "مسارات الاعتماد", "/approvals/routes", "ApprovalWorkflowService", "تعريف خطوات الاعتماد حسب نوع العملية.", "مصمم خطوات موافقة.", 9),
            Page("approvals.pending", "اعتمادات بانتظار الإجراء", "/approvals/pending", "ApprovalWorkflowService", "قائمة كل الاعتمادات المطلوبة من المستخدم.", "صندوق اعتماد موحد.", 10)),
        Module("messaging", "البريد والتنبيهات", "Messaging", "البريد الداخلي، القوالب، الإشعارات، وسجلات الإرسال.", 4,
            Page("mail.inbox", "البريد الداخلي", "/mail/inbox", "InternalMailService", "رسائل واردة/صادرة وقراءة وأرشفة.", "واجهة بريد داخلية.", 1),
            Page("mail.compose", "رسالة جديدة", "/mail/compose", "InternalMailService", "إنشاء رسالة لمستخدمين/أدوار مع مرفقات.", "نموذج إرسال مع مسودة.", 2),
            Page("mail.drafts", "المسودات", "/mail/drafts", "InternalMailService", "حفظ واستكمال المسودات.", "قائمة مسودات.", 3),
            Page("mail.templates", "قوالب البريد", "/mail/templates", "MessageTemplateService", "إدارة قوالب الرسائل.", "CRUD قوالب.", 4),
            Page("notifications.manage", "إدارة الإشعارات", "/notifications/manage", "NotificationService", "إنشاء وتنبيه مستخدمين داخل النظام.", "نموذج إشعار وجدول.", 5),
            Page("notifications.database", "قاعدة بيانات الإشعارات", "/notifications/database", "NotificationQueryService", "سجل الإشعارات والقراءة.", "جدول بحث وتصدير.", 6),
            Page("notifications.cancel", "إلغاء إشعار", "/notifications/cancel", "NotificationService", "إلغاء إشعار قبل/بعد الإرسال.", "قائمة إلغاء.", 7),
            Page("channels.email-log", "سجل البريد", "/channels/email-log", "ChannelLogService", "سجل EmailOutbox وحالة الإرسال.", "جدول حالة البريد.", 8),
            Page("channels.sms-log", "سجل الرسائل النصية", "/channels/sms-log", "ChannelLogService", "جاهز لاحقاً لمزود SMS.", "جدول سجلات.", 9),
            Page("channels.push", "تنبيه لحظي", "/channels/push", "NotificationService", "إرسال إشعار لحظي داخل النظام.", "نموذج إرسال.", 10)),
        Module("admin", "إدارة النظام والصلاحيات", "System Administration", "المستخدمون، الأدوار، الصلاحيات، الإعدادات، والسجلات.", 5,
            Page("admin.users", "مستخدمو النظام", "/admin/users", "UserAdminService", "إدارة حسابات المستخدمين وتفعيلها.", "DataGrid مستخدمين.", 1),
            Page("admin.roles", "الأدوار", "/admin/roles", "RoleAdminService", "إدارة الأدوار.", "CRUD أدوار.", 2),
            Page("admin.permissions", "الصلاحيات التفصيلية", "/admin/permissions", "PermissionService", "مصفوفة صلاحيات حسب صفحة/إجراء.", "Matrix checkboxes.", 3),
            Page("admin.login-audit", "سجلات الدخول", "/admin/login-audit", "SecurityAuditService", "سجل دخول وخروج ومحاولات فاشلة.", "جدول أمني.", 4),
            Page("admin.settings", "إعدادات النظام", "/admin/settings", "SystemSettingsService", "إعدادات عامة ومقدمي خدمات.", "نموذج إعدادات.", 5),
            Page("admin.smtp", "إعدادات البريد", "/admin/smtp", "SystemSettingsService", "تهيئة SMTP واختبار الإرسال.", "نموذج آمن بدون عرض كلمة المرور.", 6),
            Page("admin.files", "إدارة الملفات", "/admin/files", "FileAssetService", "إدارة الملفات والمرفقات المشتركة.", "مكتبة ملفات.", 7),
            Page("admin.audit-log", "سجل التدقيق", "/admin/audit-log", "AuditLogService", "عرض عمليات الإنشاء والتحديث.", "جدول Audit.", 8),
            Page("admin.jobs", "العمليات المجدولة", "/admin/jobs", "BackgroundJobService", "متابعة Hangfire والمهام.", "لوحة وظائف.", 9),
            Page("admin.modules", "خطة خدمات النظام", "/system/modules", "SystemCatalogService", "تتبع الصفحات والخدمات المخططة.", "Workbench للتنفيذ.", 10, SystemPageStatus.InProgress))
    ];

    private static SystemModuleSeed Module(string key, string nameAr, string nameEn, string description, int priority, params SystemPageSeed[] pages) =>
        new(key, nameAr, nameEn, description, priority, pages);

    private static SystemPageSeed Page(string key, string nameAr, string route, string serviceName, string servicePlan, string uiPlan, int sortOrder, SystemPageStatus status = SystemPageStatus.Planned) =>
        new(key, nameAr, route, $"system.{key}", serviceName, servicePlan, uiPlan, sortOrder, status);

    private record SystemModuleSeed(string Key, string NameAr, string NameEn, string Description, int Priority, IReadOnlyList<SystemPageSeed> Pages);
    private record SystemPageSeed(string Key, string NameAr, string Route, string PermissionKey, string ServiceName, string ServicePlan, string UiPlan, int SortOrder, SystemPageStatus Status);
}
