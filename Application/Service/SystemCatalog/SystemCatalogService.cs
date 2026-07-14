using Application.Abstraction;
using Application.Contracts.SystemCatalog;
using Domain;
using Domain.Auditing;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.SystemCatalog;

public class SystemCatalogService(ApplicationDbcontext dbcontext, ICurrentUserContext? currentUserContext = null) : ISystemCatalogService
{
    private static readonly Error PageNotFound = new("SystemCatalog.PageNotFound", "System page was not found.", StatusCodes.Status404NotFound);
    private static readonly IReadOnlyDictionary<string, CatalogPageImplementation> ImplementedRafedPages =
        new Dictionary<string, CatalogPageImplementation>(StringComparer.OrdinalIgnoreCase)
        {
            ["profiles_create.php"] = Implemented("/beneficiaries/create", "BeneficiaryService", "Create beneficiary profiles with generated numbers and contact/classification data."),
            ["profiles_update.php"] = Implemented("/beneficiaries/update", "BeneficiaryService", "Update beneficiary profile details."),
            ["profiles_remove.php"] = Implemented("/beneficiaries/update", "BeneficiaryService", "Archive beneficiary profiles with reason tracking."),
            ["profiles_database.php"] = Implemented("/beneficiaries", "BeneficiaryService", "Search and filter beneficiary profile database."),
            ["profiles_relatives_database.php"] = Implemented("/beneficiaries/dependents", "BeneficiaryService", "Create and list dependents attached to beneficiary profiles."),
            ["profiles_update_status.php"] = Implemented("/beneficiaries/update", "BeneficiaryService", "Update beneficiary profile status."),
            ["profiles_update_grade.php"] = Implemented("/beneficiaries/update", "BeneficiaryService", "Update beneficiary category and grade."),
            ["profiles_update_grade_relative.php"] = Implemented("/beneficiaries/dependents", "BeneficiaryService", "Update and review dependent category and grade data."),
            ["profiles_deleted.php"] = Implemented("/beneficiaries", "BeneficiaryService", "Review archived beneficiary profiles via status filter."),
            ["profiles_relatives_deleted.php"] = Implemented("/beneficiaries/dependents", "BeneficiaryService", "Review inactive or deleted dependent records."),
            ["profiles_cards.php"] = Implemented("/beneficiaries/cards", "BeneficiaryService", "Create and issue beneficiary card export records."),
            ["profiles_barcode.php"] = Implemented("/beneficiaries/cards", "BeneficiaryService", "Create and issue beneficiary barcode records."),
            ["profiles_external.php"] = Implemented("/beneficiaries/external", "BeneficiaryService", "Track beneficiary website join requests."),
            ["profiles_search.php"] = Implemented("/beneficiaries/external", "BeneficiaryService", "Register beneficiary association-search records."),
            ["guardians_manage.php"] = Implemented("/beneficiaries/guardians", "BeneficiaryService", "Create guardian records and mark primary guardians."),
            ["guardians_database.php"] = Implemented("/beneficiaries/guardians", "BeneficiaryService", "List guardian records attached to beneficiary profiles."),
            ["guardians_convert_from.php"] = Implemented("/beneficiaries/guardian-operations", "BeneficiaryService", "Create and decide beneficiary-to-guardian conversion operations."),
            ["guardians_convert_to.php"] = Implemented("/beneficiaries/guardian-operations", "BeneficiaryService", "Create and decide guardian-to-beneficiary conversion operations."),
            ["guardians_remove.php"] = Implemented("/beneficiaries/guardian-operations", "BeneficiaryService", "Soft-delete guardian records with decision history."),
            ["guardians_deleted.php"] = Implemented("/beneficiaries/guardian-operations", "BeneficiaryService", "Review guardian deletion operation history."),
            ["update_task_create.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Create grouped beneficiary data-update batches."),
            ["update_task_manage.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Manage grouped beneficiary data-update batches."),
            ["update_tasks_approve.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Approve and progress grouped beneficiary update batches."),
            ["update_tasks.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Track grouped beneficiary update batches."),
            ["update_self_create.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Create self-service beneficiary update batches."),
            ["update_self_requests.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Track received self-service update requests."),
            ["update_self_tasks.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Track self-service update approval tasks."),
            ["update_request_tasks.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Track update tasks linked to aid requests."),
            ["update_manual_status.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Track manual beneficiary data-status updates."),
            ["update_report_none.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Create and review no-update beneficiary reports."),
            ["update_report_wrong.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Create and review wrong-update beneficiary reports."),
            ["update_report_self.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Create and review self-update beneficiary reports."),
            ["update_report_field.php"] = Implemented("/beneficiaries/update-batches", "BeneficiaryService", "Create and review field-update beneficiary reports."),
            ["gehat_accounts.php"] = Implemented("/beneficiaries/entities", "BeneficiaryService", "Create and update beneficiary entity accounts."),
            ["gehat_representatives_accounts.php"] = Implemented("/beneficiaries/entities", "BeneficiaryService", "Maintain beneficiary entity representative contact details."),
            ["gehat_remove.php"] = Implemented("/beneficiaries/entities", "BeneficiaryService", "Archive beneficiary entity accounts by status."),
            ["gehat_restore.php"] = Implemented("/beneficiaries/entities", "BeneficiaryService", "Restore beneficiary entity accounts by status."),
            ["gehat_database.php"] = Implemented("/beneficiaries/entities", "BeneficiaryService", "Search beneficiary entity records."),
            ["gehat_representatives_database.php"] = Implemented("/beneficiaries/entities", "BeneficiaryService", "Search beneficiary entity representative records."),
            ["gehat_accounts_external.php"] = Implemented("/beneficiaries/entities", "BeneficiaryService", "Track pending beneficiary entity registrations."),
            ["request_create.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Create aid requests for beneficiary profiles or entities."),
            ["request_update.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Update aid request workflow status and notes."),
            ["request_approval_initial.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Record initial aid request approval."),
            ["request_note.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Track aid requests rejected with notes."),
            ["request_approval_search_task.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Move aid requests to social research task status."),
            ["request_approval_search_opinion.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Record social researcher opinion."),
            ["request_approval_manager.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Record manager approval for aid requests."),
            ["request_approval_committee.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Record committee approval for aid requests."),
            ["request_approval_transfer.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Transfer aid requests to another department status."),
            ["request_zap_database.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "List and filter aid request database."),
            ["request_external.php"] = Implemented("/beneficiary-services/aid-requests", "BeneficiaryServicesService", "Create and track external website aid requests."),
            ["order_finance.php"] = Implemented("/beneficiary-services/payment-orders", "BeneficiaryServicesService", "Create and manage financial payment orders."),
            ["order_storage.php"] = Implemented("/beneficiary-services/payment-orders", "BeneficiaryServicesService", "Create and manage in-kind storage payment orders."),
            ["order_manage_rejected.php"] = Implemented("/beneficiary-services/payment-orders", "BeneficiaryServicesService", "Track payment orders rejected with notes."),
            ["order_zap_rejected.php"] = Implemented("/beneficiary-services/payment-orders", "BeneficiaryServicesService", "Track finally rejected payment orders."),
            ["order_zap_remove.php"] = Implemented("/beneficiary-services/payment-orders", "BeneficiaryServicesService", "Remove payment orders by status."),
            ["order_zap_close.php"] = Implemented("/beneficiary-services/payment-orders", "BeneficiaryServicesService", "Close payment orders."),
            ["order_zap_database.php"] = Implemented("/beneficiary-services/payment-orders", "BeneficiaryServicesService", "List and filter payment order database."),
            ["kafeel_users_manage.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Create and update sponsor accounts."),
            ["kafeel_users_database.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "List sponsor accounts."),
            ["kafeel_users_remove.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Mark sponsor accounts removed."),
            ["kafeel_users_removed.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "List removed sponsor accounts."),
            ["kafeel_requires_manage.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Create and update sponsorship requirements."),
            ["kafeel_requires_database.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "List sponsorship requirements."),
            ["kafeel_records.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Create and manage sponsorship records."),
            ["kafeel_records_expired.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Track expired sponsorship records."),
            ["kafeel_records_due_collection.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Track sponsorship records due for collection."),
            ["kafeel_records_remove.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Remove sponsorship records by status."),
            ["kafeel_records_removed.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "List removed sponsorship records."),
            ["kafeel_records_database.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "List sponsorship record database."),
            ["kafeel_records_reassign.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Track reassigned sponsorship records."),
            ["kafeel_assignments.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Create sponsorship payment assignments."),
            ["kafeel_assignments_due.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Track due sponsorship assignments."),
            ["kafeel_assignments_canceled.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Track cancelled sponsorship assignments."),
            ["kafeel_assignments_expired.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Track expired sponsorship assignments."),
            ["kafeel_payment_requests.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Create sponsorship payment requests."),
            ["kafeel_payment_rejected.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Track rejected sponsorship payments."),
            ["kafeel_payment_database.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "List sponsorship payment database."),
            ["kafeel_payment_close.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Close sponsorship payments."),
            ["kafala_payment_report.php"] = Implemented("/beneficiary-services/sponsorships", "BeneficiaryServicesService", "Display sponsorship payment records and balances."),
            ["gehat_supports.php"] = Implemented("/beneficiary-services/entity-supports", "BeneficiaryServicesService", "Create and decide entity support requests."),
            ["gehat_supports_external.php"] = Implemented("/beneficiary-services/entity-supports", "BeneficiaryServicesService", "Create and track external entity support requests."),
            ["coupons_require.php"] = Implemented("/beneficiary-services/coupons", "BeneficiaryServicesService", "Create coupon requirements."),
            ["coupons_issue.php"] = Implemented("/beneficiary-services/coupons", "BeneficiaryServicesService", "Issue coupons."),
            ["coupons_approved.php"] = Implemented("/beneficiary-services/coupons", "BeneficiaryServicesService", "Approve coupons."),
            ["coupons_deliver.php"] = Implemented("/beneficiary-services/coupons", "BeneficiaryServicesService", "Deliver coupons."),
            ["hr_personnel_create.php"] = Implemented("/hr/employees/create", "HumanResourceService", "Create employee accounts with department, title, salary, and contact data."),
            ["hr_personnel_update.php"] = Implemented("/hr/employees", "HumanResourceService", "Search and update employee profile data."),
            ["hr_personnel_database.php"] = Implemented("/hr/employees", "HumanResourceService", "Search employee profile database."),
            ["hr_volunteer_create.php"] = Implemented("/hr/volunteers", "HumanResourceService", "Create volunteer employee accounts."),
            ["hr_volunteer_update.php"] = Implemented("/hr/volunteers", "HumanResourceService", "Update volunteer employee accounts through the shared employee editor."),
            ["hr_volunteer_database.php"] = Implemented("/hr/volunteers", "HumanResourceService", "Search volunteer employee records."),
            ["hr_personnel_type.php"] = Implemented("/hr/volunteers", "HumanResourceService", "Manage employee versus volunteer account type."),
            ["hr_personnel_warnings.php"] = Implemented("/hr/discipline", "HumanResourceService", "Create and decide employee warning records."),
            ["hr_personnel_questions.php"] = Implemented("/hr/discipline", "HumanResourceService", "Create and decide employee questioning records."),
            ["hr_personnel_vacations_manage.php"] = Implemented("/hr/leave-balances", "HumanResourceService", "Create and update annual leave balances."),
            ["hr_personnel_vacations.php"] = Implemented("/hr/leaves", "HumanResourceService", "Create and decide employee leave requests."),
            ["hr_personnel_evaluation.php"] = Implemented("/hr/evaluations", "HumanResourceService", "Create and list employee evaluations."),
            ["hr_personnel_resign.php"] = Implemented("/hr/employees", "HumanResourceService", "Terminate and restore employee service status."),
            ["hr_personnel_cards.php"] = Implemented("/hr/cards", "HumanResourceService", "Issue employee cards."),
            ["hr_personnel_testahel.php"] = Implemented("/hr/cards", "HumanResourceService", "Issue Testahel cards."),
            ["hr_personnel_letter.php"] = Implemented("/hr/letters", "HumanResourceService", "Create and issue personal HR letters."),
            ["hr_letters_general.php"] = Implemented("/hr/letters", "HumanResourceService", "Create and issue general HR letters."),
            ["hr_salaries_export.php"] = Implemented("/hr/payroll", "HumanResourceService", "Generate payroll preview records."),
            ["demands_zap_approve.php"] = Implemented("/hr/admin-requests", "HumanResourceService", "Approve or reject administrative requests."),
            ["demands_zap_managed.php"] = Implemented("/hr/admin-requests", "HumanResourceService", "Review administrative requests managed by HR."),
            ["demands_zap_database.php"] = Implemented("/hr/admin-requests", "HumanResourceService", "Search administrative request records."),
            ["attendance_settings.php"] = Implemented("/hr/attendance-settings", "HumanResourceService", "Manage attendance policies."),
            ["attendance_locations.php"] = Implemented("/hr/attendance-settings", "HumanResourceService", "Manage attendance registration locations."),
            ["attendance_official_vacations.php"] = Implemented("/hr/attendance-settings", "HumanResourceService", "Manage official vacations."),
            ["attendance_record.php"] = Implemented("/hr/attendance", "HumanResourceService", "Record employee attendance."),
            ["attendance_execuses.php"] = Implemented("/hr/attendance-excuses", "HumanResourceService", "Create and decide attendance excuses."),
            ["attendance_present.php"] = Implemented("/hr/presence", "HumanResourceService", "Show current present employees."),
            ["attendance_report.php"] = Implemented("/hr/attendance", "HumanResourceService", "Search attendance records."),
            ["attendance_report_general.php"] = Implemented("/hr/attendance", "HumanResourceService", "Report attendance records."),
            ["attendance_vacation_credit.php"] = Implemented("/hr/leave-balances", "HumanResourceService", "Show employee vacation credit records."),
            ["hr_safety_categories.php"] = Implemented("/hr/safety", "HumanResourceService", "Manage occupational risk categories."),
            ["hr_safety_operations.php"] = Implemented("/hr/safety", "HumanResourceService", "Manage safety procedures."),
            ["hr_safety_answers.php"] = Implemented("/hr/safety", "HumanResourceService", "Record safety procedure forms."),
            ["hr_safety_report.php"] = Implemented("/hr/safety", "HumanResourceService", "Report safety inspections and procedures."),
            ["hr_request_create.php"] = Implemented("/hr/recruitment", "HumanResourceService", "Create recruitment requests."),
            ["hr_request_approved.php"] = Implemented("/hr/recruitment", "HumanResourceService", "Announce approved recruitment requests."),
            ["hr_request_received.php"] = Implemented("/hr/recruitment", "HumanResourceService", "Track received recruitment candidates."),
            ["hr_request_completed.php"] = Implemented("/hr/recruitment", "HumanResourceService", "Complete recruitment requests."),
            ["hr_request_interview.php"] = Implemented("/hr/recruitment", "HumanResourceService", "Record candidate interview details."),
            ["projects_create.php"] = Implemented("/programs-projects/create", "ProgramsProjectsService", "Create project, program, design, or qualification records with generated project codes."),
            ["projects_update.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Search project records and update operational status."),
            ["projects_database.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Search, filter, complete, reopen, archive, and restore project records."),
            ["projects_tasks.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Create and list project tasks with owner, due date, status, and progress."),
            ["projects_milestones.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Create and list project milestones and activities."),
            ["projects_contracts.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Create and list project contracts linked to suppliers."),
            ["projects_income.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Record project income entries."),
            ["projects_expenses.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Record project expense entries."),
            ["projects_assigned_users.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Attach beneficiaries or participant records to a project."),
            ["projects_assigned_gehats.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Attach entity records to a project."),
            ["projects_reports.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Create and list project follow-up reports."),
            ["projects_dashboard.php"] = Implemented("/programs-projects", "ProgramsProjectsService", "Display project dashboard counts, balances, and latest records."),
            ["projects_followup.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Track execution entries, assignments, and reports."),
            ["projects_gantt.php"] = Implemented("/programs-projects/monitoring", "ProgramsProjectsService", "Display milestone timeline with progress values."),
            ["projects_kanban.php"] = Implemented("/programs-projects/monitoring", "ProgramsProjectsService", "Display project tasks grouped by Kanban status."),
            ["projects_pm_indicators.php"] = Implemented("/programs-projects/monitoring", "ProgramsProjectsService", "Display project KPI counters and financial balance indicators."),
            ["projects_statistics.php"] = Implemented("/programs-projects", "ProgramsProjectsService", "Display project statistics through the dashboard."),
            ["projects_databases.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Search and manage project database records."),
            ["projects_balance_transfer.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Record balance transfer finance entries."),
            ["projects_complete.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Mark projects as completed."),
            ["projects_open.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Reopen completed projects."),
            ["projects_delete.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Archive projects using deleted status."),
            ["projects_restore.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Restore archived projects."),
            ["suppliers_accounts.php"] = Implemented("/programs-projects/suppliers", "ProgramsProjectsService", "Create and update supplier accounts."),
            ["suppliers_database.php"] = Implemented("/programs-projects/suppliers", "ProgramsProjectsService", "Search supplier records."),
            ["suppliers_remove.php"] = Implemented("/programs-projects/suppliers", "ProgramsProjectsService", "Archive supplier accounts."),
            ["suppliers_restore.php"] = Implemented("/programs-projects/suppliers", "ProgramsProjectsService", "Restore supplier accounts."),
            ["program_idea_create.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Create program idea proposals."),
            ["program_idea_pending.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter program ideas pending approval."),
            ["program_idea_marketing.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Move and filter program ideas in marketing status."),
            ["program_idea_approved.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Approve and list active program ideas."),
            ["program_idea_note.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Record note status and decision notes for program ideas."),
            ["program_idea_rejected.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Reject and list rejected program ideas."),
            ["program_idea_cancelled.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Cancel and list cancelled program ideas."),
            ["program_idea_completed.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Complete and list completed program ideas."),
            ["program_approval_idea.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Create and decide idea approvals."),
            ["program_approval_update_team.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Create and decide team update approvals."),
            ["program_approval_team.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Create and decide team approvals."),
            ["program_approval_update_budget.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Create and decide budget update approvals."),
            ["program_approval_budget.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Create and decide budget approvals."),
            ["program_approval_marketing.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Create and decide marketing approvals."),
            ["program_approval_marketing_complete.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Complete marketing approval decisions."),
            ["program_approval_complete.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "Complete approval decisions and sync linked idea status."),
            ["program_approval_history.php"] = Implemented("/programs-projects/approvals", "ProgramsProjectsService", "List approval history and decision notes."),
            ["program_manage_dashboard.php"] = Implemented("/programs-projects", "ProgramsProjectsService", "Display program dashboard totals with links to registration, attendance, certificate, survey, and qualification workflows."),
            ["program_manage_register.php"] = Implemented("/programs-projects/registration", "ProgramsProjectsService", "Create program registration records."),
            ["program_manage_register_database.php"] = Implemented("/programs-projects/registration", "ProgramsProjectsService", "List and filter program registration records."),
            ["program_manage_requests.php"] = Implemented("/programs-projects/registration", "ProgramsProjectsService", "Approve, reject, cancel, and mark program registrations attended."),
            ["program_manage_schedule.php"] = Implemented("/programs-projects/attendance", "ProgramsProjectsService", "Create and list scheduled program sessions."),
            ["program_manage_attendance.php"] = Implemented("/programs-projects/attendance", "ProgramsProjectsService", "Record present, absent, and excused program attendance."),
            ["program_manage_certificates_issue.php"] = Implemented("/programs-projects/certificates-surveys", "ProgramsProjectsService", "Issue program certificates with generated certificate numbers."),
            ["program_manage_followup.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Track program execution and follow-up reports."),
            ["program_manage_reports.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Create and list program reports."),
            ["program_manage_survey.php"] = Implemented("/programs-projects/certificates-surveys", "ProgramsProjectsService", "Create program surveys with JSON question definitions."),
            ["program_manage_survey_records.php"] = Implemented("/programs-projects/certificates-surveys", "ProgramsProjectsService", "Save and list program survey submissions."),
            ["program_zap_special.php"] = Implemented("/programs-projects/program-publishing", "ProgramsProjectsService", "Manage special program category and registration form metadata."),
            ["program_zap_update.php"] = Implemented("/programs-projects/program-publishing", "ProgramsProjectsService", "Update special program publishing and registration form settings."),
            ["program_zap_cancel.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Cancel or archive program records through status management."),
            ["program_zap_database_pending.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter pending program proposals."),
            ["program_zap_database_marketing.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter program proposals in marketing."),
            ["program_zap_database_approved.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter approved program proposals."),
            ["program_zap_database_completed.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter completed program proposals."),
            ["program_zap_database_note.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter program proposals with note status."),
            ["program_zap_database_rejected.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter rejected program proposals."),
            ["program_zap_database_cancelled.php"] = Implemented("/programs-projects/ideas", "ProgramsProjectsService", "Filter cancelled program proposals."),
            ["program_zap_database.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Search program and project records."),
            ["program_zap_registered.php"] = Implemented("/programs-projects/registration", "ProgramsProjectsService", "List registered program participants and decisions."),
            ["program_zap_statistics.php"] = Implemented("/programs-projects", "ProgramsProjectsService", "Display program statistics through dashboard metrics."),
            ["program_zap_special_database.php"] = Implemented("/programs-projects/program-publishing", "ProgramsProjectsService", "List special program records with publish and form status."),
            ["program_manager_dashboard.php"] = Implemented("/programs-projects", "ProgramsProjectsService", "Display manager dashboard metrics for programs and projects."),
            ["program_manager_update.php"] = Implemented("/programs-projects/projects", "ProgramsProjectsService", "Update program records through the shared project database."),
            ["program_manager_tasks.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Create and list program tasks."),
            ["program_manager_tasks_new.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Track new program tasks through task status."),
            ["program_manager_tasks_running.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Track running program tasks through task status."),
            ["program_manager_tasks_failed.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Track blocked program tasks through task status."),
            ["program_manager_tasks_completed.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Track completed program tasks through task status."),
            ["program_manager_tasks_finished.php"] = Implemented("/programs-projects/planning", "ProgramsProjectsService", "Track finished program tasks through task status."),
            ["program_manager_publish.php"] = Implemented("/programs-projects/program-publishing", "ProgramsProjectsService", "Publish and unpublish program records."),
            ["program_manager_certificates_templates.php"] = Implemented("/programs-projects/certificates-surveys", "ProgramsProjectsService", "Create and list program certificate templates."),
            ["program_manager_statistics.php"] = Implemented("/programs-projects", "ProgramsProjectsService", "Display program statistics through dashboard metrics."),
            ["program_manager_custody.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Record custody finance entries."),
            ["program_manager_expenses.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Record program expense entries."),
            ["program_manager_income.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "Record program income entries."),
            ["program_manager_financials.php"] = Implemented("/programs-projects/execution", "ProgramsProjectsService", "List program income, expense, custody, and transfer entries."),
            ["program_manager_forms.php"] = Implemented("/programs-projects/program-publishing", "ProgramsProjectsService", "Save program registration form JSON metadata."),
            ["qual_required.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Create qualification cases for beneficiaries needing support."),
            ["qual_opinion.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Record qualification management opinion and decision state."),
            ["qual_projects.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Create qualification project cases linked to optional project records."),
            ["qual_projects_database.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "List qualification cases and linked projects."),
            ["qual_installments.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Create qualification installment schedules."),
            ["qual_late.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Track late qualification installments."),
            ["qual_paid.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Record and list paid qualification installments."),
            ["qual_qualified.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Track qualification outcome status."),
            ["qual_reports.php"] = Implemented("/programs-projects/qualification", "ProgramsProjectsService", "Display qualification cases, installments, balances, and statuses.")
        };

    public async Task<Result<SeedSystemCatalogResponse>> SeedFirstPagesAsync(CancellationToken cancellationToken = default)
    {
        var modulesCreated = 0;
        var groupsCreated = 0;
        var pagesCreated = 0;
        var pagesUpdated = 0;

        var seedModuleKeys = RafedCatalogSeed.Modules.Select(x => x.Key).ToHashSet();
        var seedGroupKeys = RafedCatalogSeed.Modules.SelectMany(x => x.Groups).Select(x => x.Key).ToHashSet();
        var seedPageKeys = RafedCatalogSeed.Modules.SelectMany(x => x.Groups).SelectMany(x => x.Pages).Select(x => x.Key).ToHashSet();

        var obsoletePages = await dbcontext.SystemPages.Where(x => !seedPageKeys.Contains(x.Key)).ToListAsync(cancellationToken);
        var obsoleteGroups = await dbcontext.SystemPageGroups.Where(x => !seedGroupKeys.Contains(x.Key)).ToListAsync(cancellationToken);
        var obsoleteModules = await dbcontext.SystemModules.Where(x => !seedModuleKeys.Contains(x.Key)).ToListAsync(cancellationToken);

        dbcontext.SystemPages.RemoveRange(obsoletePages);
        dbcontext.SystemPageGroups.RemoveRange(obsoleteGroups);
        dbcontext.SystemModules.RemoveRange(obsoleteModules);
        if (obsoletePages.Count > 0 || obsoleteGroups.Count > 0 || obsoleteModules.Count > 0)
            await dbcontext.SaveChangesAsync(cancellationToken);

        var modules = await dbcontext.SystemModules
            .Include(x => x.Groups)
            .Include(x => x.Pages)
            .ToDictionaryAsync(x => x.Key, cancellationToken);

        foreach (var moduleSeed in RafedCatalogSeed.Modules)
        {
            if (!modules.TryGetValue(moduleSeed.Key, out var module))
            {
                module = new SystemModule { Key = moduleSeed.Key };
                dbcontext.SystemModules.Add(module);
                modules[moduleSeed.Key] = module;
                modulesCreated++;
            }

            module.NameAr = moduleSeed.NameAr;
            module.NameEn = moduleSeed.NameEn;
            module.Description = moduleSeed.Description;
            module.IconCss = moduleSeed.IconCss;
            module.Priority = moduleSeed.SortOrder;

            var existingGroups = module.Groups.ToDictionary(x => x.Key);
            var existingPages = module.Pages.ToDictionary(x => x.Key);

            foreach (var groupSeed in moduleSeed.Groups)
            {
                if (!existingGroups.TryGetValue(groupSeed.Key, out var group))
                {
                    group = new SystemPageGroup { Key = groupSeed.Key, SystemModule = module };
                    dbcontext.SystemPageGroups.Add(group);
                    existingGroups[groupSeed.Key] = group;
                    groupsCreated++;
                }

                group.NameAr = groupSeed.NameAr;
                group.SortOrder = groupSeed.SortOrder;

                foreach (var pageSeed in groupSeed.Pages)
                {
                    var implementation = ResolveImplementation(pageSeed);
                    var isNewPage = !existingPages.TryGetValue(pageSeed.Key, out var existingPage);
                    SystemPage page;
                    if (isNewPage)
                    {
                        page = new SystemPage { Key = pageSeed.Key, SystemModule = module };
                        dbcontext.SystemPages.Add(page);
                        existingPages[pageSeed.Key] = page;
                        pagesCreated++;
                    }
                    else
                    {
                        page = existingPage!;
                        pagesUpdated++;
                    }

                    page.SystemModule = module;
                    page.SystemPageGroup = group;
                    page.SystemModuleId = module.Id;
                    page.NameAr = pageSeed.NameAr;
                    page.Route = implementation?.Route ?? pageSeed.Route;
                    page.PermissionKey = pageSeed.PermissionKey;
                    page.ServiceName = implementation?.ServiceName ?? pageSeed.ServiceName;
                    page.ServicePlan = implementation?.ServicePlan ?? pageSeed.ServicePlan;
                    page.UiPlan = implementation?.UiPlan ?? pageSeed.UiPlan;
                    page.OriginalHref = pageSeed.OriginalHref;
                    page.OriginalIcon = pageSeed.OriginalIcon;
                    page.SortOrder = pageSeed.SortOrder;

                    if (implementation is not null)
                    {
                        page.Status = implementation.Status;
                    }
                    else if (isNewPage || page.Status == SystemPageStatus.Planned && pageSeed.Status == SystemPageStatus.Implemented)
                    {
                        page.Status = pageSeed.Status;
                    }
                }
            }
        }

        await dbcontext.SaveChangesAsync(cancellationToken);

        var catalogPermissions = await dbcontext.SystemPages
            .AsNoTracking()
            .Include(x => x.SystemModule)
            .Select(x => new { x.PermissionKey, x.NameAr, Category = x.SystemModule!.NameAr, x.Route })
            .ToListAsync(cancellationToken);
        var existingPermissionKeys = await dbcontext.AppPermissions
            .Select(x => x.Key)
            .ToHashSetAsync(cancellationToken);
        foreach (var permission in catalogPermissions.Where(x => !existingPermissionKeys.Contains(x.PermissionKey)))
        {
            dbcontext.AppPermissions.Add(new AppPermission
            {
                Key = permission.PermissionKey,
                NameAr = permission.NameAr,
                Category = permission.Category,
                Description = permission.Route
            });
        }
        if (dbcontext.ChangeTracker.HasChanges())
            await dbcontext.SaveChangesAsync(cancellationToken);

        var totalModules = await dbcontext.SystemModules.CountAsync(cancellationToken);
        var totalGroups = await dbcontext.SystemPageGroups.CountAsync(cancellationToken);
        var totalPages = await dbcontext.SystemPages.CountAsync(cancellationToken);

        return Result.Success(new SeedSystemCatalogResponse(
            modulesCreated,
            groupsCreated,
            pagesCreated,
            pagesUpdated,
            totalModules,
            totalGroups,
            totalPages));
    }

    public async Task<Result<IEnumerable<SystemModuleResponse>>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        var modules = await dbcontext.SystemModules
            .AsNoTracking()
            .Include(x => x.Groups)
            .Include(x => x.Pages)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.NameAr)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SystemModuleResponse>>(modules.Select(MapModule));
    }

    public async Task<Result<IEnumerable<SystemPageGroupResponse>>> GetGroupsAsync(string? moduleKey = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SystemPageGroups
            .AsNoTracking()
            .Include(x => x.SystemModule)
            .Include(x => x.Pages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(moduleKey))
            query = query.Where(x => x.SystemModule != null && x.SystemModule.Key == moduleKey);

        var groups = await query
            .OrderBy(x => x.SystemModule!.Priority)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SystemPageGroupResponse>>(groups.Select(MapGroup));
    }

    public async Task<Result<IEnumerable<SystemPageResponse>>> GetPagesAsync(string? moduleKey = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SystemPages
            .AsNoTracking()
            .Include(x => x.SystemModule)
            .Include(x => x.SystemPageGroup)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(moduleKey))
            query = query.Where(x => x.SystemModule != null && x.SystemModule.Key == moduleKey);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SystemPageStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);

        var pages = await query
            .OrderBy(x => x.SystemModule!.Priority)
            .ThenBy(x => x.SystemPageGroup == null ? 0 : x.SystemPageGroup.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SystemPageResponse>>(pages.Select(MapPage));
    }

    public async Task<Result<SystemPageResponse>> GetPageAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await dbcontext.SystemPages
            .AsNoTracking()
            .Include(x => x.SystemModule)
            .Include(x => x.SystemPageGroup)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return page is null ? Result.Failure<SystemPageResponse>(PageNotFound) : Result.Success(MapPage(page));
    }

    public async Task<Result<SystemPageResponse>> GetPageByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var page = await dbcontext.SystemPages
            .AsNoTracking()
            .Include(x => x.SystemModule)
            .Include(x => x.SystemPageGroup)
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        return page is null ? Result.Failure<SystemPageResponse>(PageNotFound) : Result.Success(MapPage(page));
    }

    public async Task<Result<IEnumerable<SystemNavigationModuleResponse>>> GetNavigationAsync(CancellationToken cancellationToken = default)
    {
        var grantedKeys = await GetGrantedPermissionKeysAsync(cancellationToken);
        var modules = await dbcontext.SystemModules
            .AsNoTracking()
            .Include(x => x.Groups)
            .ThenInclude(x => x.Pages)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.NameAr)
            .ToListAsync(cancellationToken);

        var navigation = modules.Select(module =>
            new SystemNavigationModuleResponse(
                module.Key,
                module.NameAr,
                module.Description,
                module.IconCss,
                module.Priority,
                module.Groups
                    .OrderBy(group => group.SortOrder)
                    .Select(group =>
                        new SystemNavigationGroupResponse(
                            group.Key,
                            group.NameAr,
                            group.SortOrder,
                            group.Pages
                                .Where(page => grantedKeys is null || grantedKeys.Contains(page.PermissionKey))
                                .OrderBy(page => page.SortOrder)
                                .Select(page =>
                                    new SystemNavigationPageResponse(
                                        page.Key,
                                        page.NameAr,
                                        page.Route,
                                        page.PermissionKey,
                                        page.Status.ToString(),
                                        page.OriginalHref,
                                        page.OriginalIcon,
                                        page.SortOrder))
                                .ToList()))
                    .ToList()));

        return Result.Success<IEnumerable<SystemNavigationModuleResponse>>(navigation);
    }

    public async Task<Result<CatalogRouteAccessResponse>> GetRouteAccessAsync(string route, CancellationToken cancellationToken = default)
    {
        var normalizedRoute = NormalizeRoute(route);
        if (string.IsNullOrWhiteSpace(normalizedRoute))
            return Result.Success(new CatalogRouteAccessResponse(false, true, []));

        var pages = await dbcontext.SystemPages.AsNoTracking()
            .Where(x => x.Route == normalizedRoute)
            .Select(x => x.PermissionKey)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (pages.Count == 0)
            return Result.Success(new CatalogRouteAccessResponse(false, true, []));

        var grantedKeys = await GetGrantedPermissionKeysAsync(cancellationToken);
        var isAllowed = grantedKeys is null || pages.Any(grantedKeys.Contains);
        if (!isAllowed && currentUserContext is not null)
        {
            var actor = currentUserContext.UserId ?? "unknown";
            var recentCutoff = DateTime.UtcNow.AddHours(3).AddMinutes(-1);
            var alreadyAudited = await dbcontext.AuditLogs.AsNoTracking().AnyAsync(x =>
                x.ActorUserId == actor && x.Action == "PermissionDenied" && x.EntityName == "CatalogRoute" &&
                x.EntityId == normalizedRoute && x.CreatedAt >= recentCutoff, cancellationToken);
            if (!alreadyAudited)
            {
                dbcontext.AuditLogs.Add(new AuditLog
                {
                    ActorUserId = actor,
                    Action = "PermissionDenied",
                    EntityName = "CatalogRoute",
                    EntityId = normalizedRoute,
                    Details = $"Required one of: {string.Join(", ", pages)}; ip={currentUserContext.RemoteIpAddress ?? "unknown"}"
                });
                await dbcontext.SaveChangesAsync(cancellationToken);
            }
        }
        return Result.Success(new CatalogRouteAccessResponse(true, isAllowed, pages));
    }

    private async Task<HashSet<string>?> GetGrantedPermissionKeysAsync(CancellationToken cancellationToken)
    {
        if (currentUserContext is null) return null;
        if (currentUserContext.Roles.Contains("Admin")) return null;
        var roles = currentUserContext.Roles.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (roles.Count == 0) return [];
        var keys = await dbcontext.RolePermissions.AsNoTracking()
            .Where(x => x.IsGranted && x.Role != null && roles.Contains(x.Role.Name!) && x.AppPermission != null)
            .Select(x => x.AppPermission!.Key)
            .ToListAsync(cancellationToken);
        return keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return string.Empty;

        var path = route.Trim();
        var queryIndex = path.IndexOfAny(['?', '#']);
        if (queryIndex >= 0)
            path = path[..queryIndex];

        return "/" + path.Trim('/');
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
            module.IconCss,
            module.Priority,
            module.Groups.Count,
            module.Pages.Count,
            module.Pages.Count(x => x.Status == SystemPageStatus.Planned),
            module.Pages.Count(x => x.Status == SystemPageStatus.InProgress),
            module.Pages.Count(x => x.Status == SystemPageStatus.Implemented));

    private static SystemPageGroupResponse MapGroup(SystemPageGroup group) =>
        new(
            group.Id,
            group.SystemModuleId,
            group.SystemModule?.Key ?? string.Empty,
            group.Key,
            group.NameAr,
            group.SortOrder,
            group.Pages.Count,
            group.Pages.Count(x => x.Status == SystemPageStatus.Planned),
            group.Pages.Count(x => x.Status == SystemPageStatus.InProgress),
            group.Pages.Count(x => x.Status == SystemPageStatus.Implemented));

    private static SystemPageResponse MapPage(SystemPage page) =>
        new(
            page.Id,
            page.SystemModuleId,
            page.SystemModule?.Key ?? string.Empty,
            page.SystemModule?.NameAr ?? string.Empty,
            page.SystemPageGroupId,
            page.SystemPageGroup?.Key,
            page.SystemPageGroup?.NameAr,
            page.Key,
            page.NameAr,
            page.Route,
            page.PermissionKey,
            page.ServiceName,
            page.ServicePlan,
            page.UiPlan,
            page.OriginalHref,
            page.OriginalIcon,
            page.Status.ToString(),
            page.SortOrder);

    private static CatalogPageImplementation? ResolveImplementation(RafedPageSeed pageSeed)
    {
        if (ImplementedRafedPages.TryGetValue(pageSeed.OriginalHref, out var implementation))
            return implementation;

        return TryResolveElectronicOfficeImplementation(pageSeed)
            ?? TryResolveExecutiveSupervisionImplementation(pageSeed)
            ?? TryResolveTechEnablementImplementation(pageSeed)
            ?? TryResolveVolunteeringImplementation(pageSeed)
            ?? TryResolveDocumentationArchiveImplementation(pageSeed)
            ?? TryResolveReportsStatisticsImplementation(pageSeed)
            ?? TryResolveMovementMaintenanceImplementation(pageSeed)
            ?? TryResolveEvaluationFollowUpImplementation(pageSeed)
            ?? TryResolveInstitutionalExcellenceImplementation(pageSeed)
            ?? TryResolveFinancialDevelopmentImplementation(pageSeed)
            ?? TryResolvePublicRelationsMediaImplementation(pageSeed)
            ?? TryResolveAccountingImplementation(pageSeed);
    }

    private static CatalogPageImplementation? TryResolveElectronicOfficeImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("electronic-office.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href is "personnel_emails.php")
            return Implemented("/mail/inbox", "MessagingService", "Read internal office mail with inbox, sent, archive, read status, recipient detail, archive, and restore actions.");
        if (href is "personnel_emails_drafts.php")
            return Implemented("/mail/drafts", "MessagingService", "Create, edit, send, and cancel internal mail drafts with selected recipients.");
        if (href is "personnel_emails_templates.php")
            return Implemented("/mail/templates", "MessagingService", "Manage reusable internal mail templates.");
        if (href is "common_tasks.php")
            return Implemented("/tasks/mine", "TaskManagementService", "Review assigned office tasks with activity history and status follow-up.");
        if (href is "common_tasks_create.php")
            return Implemented("/tasks/create", "TaskManagementService", "Create office tasks for employees with priority, assignee, due date, and activity trail.");
        if (href is "common_tasks_update.php")
            return Implemented("/tasks/manage", "TaskManagementService", "Manage office tasks assigned to departments and employees.");
        if (href is "common_tasks_complete.php")
            return Implemented("/tasks/complete", "TaskManagementService", "Complete office tasks and record completion activity history.");
        if (href is "common_tasks_remove.php")
            return Implemented("/tasks/delete", "TaskManagementService", "Delete office tasks with reason tracking and restoration history.");
        if (href.StartsWith("personnel_tasks", StringComparison.OrdinalIgnoreCase))
            return Implemented("/tasks/mine", "TaskManagementService", "Review personal, project, and strategy task records.");
        if (href is "personnel_attendance.php" or "personnel_reminders.php" or "personnel_calendar.php" or "personnel_questions.php" or "personnel_warnings.php" or "personnel_password.php" or "personnel_emails_preferences.php")
            return Implemented("/electronic-office/services", "ElectronicOfficeService", "Manage office attendance, personal reminders, work calendar records, questions, warnings, password-change service notes, and mail preferences.");
        if (href.StartsWith("common_", StringComparison.OrdinalIgnoreCase))
            return Implemented("/electronic-office/admin-office", "ElectronicOfficeService", "Manage administrative office evaluations, employee notifications, owned indicators, and current presence records.");
        if (href.StartsWith("demands_", StringComparison.OrdinalIgnoreCase) || href is "personnel_volunteer.php" or "personnel_purchase.php" or "personnel_car.php")
            return Implemented("/electronic-office/requests", "ElectronicOfficeService", "Manage office administrative requests for vacation, excuse, finance, general, volunteer groups, purchase, car, cancellation, and status registers.");
        if (href.StartsWith("operations_", StringComparison.OrdinalIgnoreCase))
            return Implemented("/electronic-office/transactions", "ElectronicOfficeService", "Manage internal transactions, cancellations, follow-up steps, rejected steps, managed steps, and status registers.");
        if (href.StartsWith("personnel_", StringComparison.OrdinalIgnoreCase))
            return Implemented("/electronic-office/reports", "ElectronicOfficeService", "Manage office profile, employment, tutorial, notification, vacation-credit, attendance, task, and strategy indicator logs.");

        return null;
    }

    private static CatalogPageImplementation? TryResolveExecutiveSupervisionImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("executive-supervision.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href is "corporate_docs.php" or "foundation_helper.php")
            return Implemented("/executive-supervision/foundation", "ExecutiveSupervisionService", "Manage establishment document map records and foundation-helper notes.");
        if (href is "request_credit.php" or "request_forms.php")
            return Implemented("/executive-supervision/aid-committee", "ExecutiveSupervisionService", "Manage aid committee credit movements and decision form records.");
        if (href.StartsWith("approval_expenses_", StringComparison.OrdinalIgnoreCase) || href.StartsWith("forms_", StringComparison.OrdinalIgnoreCase))
            return Implemented("/executive-supervision/approvals", "ExecutiveSupervisionService", "Manage executive approvals for payment authorizations, sponsorship expenses, payment orders, coupons, recruitment, purchase, general maintenance, and vehicle maintenance.");
        if (href.StartsWith("tameed_", StringComparison.OrdinalIgnoreCase))
            return Implemented("/executive-supervision/payment-authorizations", "ExecutiveSupervisionService", "Manage payment authorizations, rejected-with-note records, final rejected records, and authorization database.");
        if (href is "notifications_management.php")
            return Implemented("/notifications/manage", "MessagingService", "Create internal, email, SMS, and push notifications with scheduled delivery metadata and selected recipients.");
        if (href is "notifications_delete.php")
            return Implemented("/notifications/cancel", "MessagingService", "Cancel active notifications with reason tracking while preserving recipient delivery history.");
        if (href is "notifications_database.php" or "notifications_canceled.php")
            return Implemented("/notifications/database", "MessagingService", "Search notification records and follow recipient read, delivery, failure, retry, and cancellation status.");
        if (href is "task_create.php")
            return Implemented("/tasks/create", "TaskManagementService", "Create general employee tasks with priority, assignee, due date, and activity history.");
        if (href is "task_update.php")
            return Implemented("/tasks/manage", "TaskManagementService", "Manage employee task status, assignment, progress, and activity history.");
        if (href is "task_remove.php")
            return Implemented("/tasks/delete", "TaskManagementService", "Delete employee tasks with reason tracking and audit activity.");
        if (href is "task_complete.php")
            return Implemented("/tasks/complete", "TaskManagementService", "Complete employee tasks and record completion notes in the activity history.");
        if (href is "task_redirect.php")
            return Implemented("/tasks/redirect", "TaskManagementService", "Redirect stalled tasks to a new assignee with transition activity history.");
        if (href is "tasks_database.php")
            return Implemented("/tasks/database", "TaskManagementService", "Search task records and review per-task activity history and comments.");
        if (href is "task_removed.php")
            return Implemented("/tasks/restore", "TaskManagementService", "Restore deleted tasks while preserving delete and restore history.");
        if (href.StartsWith("decisions_", StringComparison.OrdinalIgnoreCase) || href.StartsWith("decision_", StringComparison.OrdinalIgnoreCase))
            return Implemented("/executive-supervision/decisions", "ExecutiveSupervisionService", "Manage administrative decisions, meeting decisions, decision tasks, and decision template exports.");

        return null;
    }

    private static CatalogPageImplementation? TryResolveTechEnablementImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("tech-enablement.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href is "system_settings.php")
            return Implemented("/admin/settings", "AdminService", "Manage service-provider and system settings through the existing admin settings workflow.");
        if (href is "system_worker_log.php")
            return Implemented("/admin/jobs", "AdminService", "Review scheduled worker logs through the existing jobs workflow.");
        if (href is "system_administrators.php")
            return Implemented("/admin/users", "AdminService", "Manage system users through the existing admin user workflow.");
        if (href is "system_register.php")
            return Implemented("/admin/login-audit", "AdminService", "Review login audit records through the existing admin audit workflow.");
        if (href is "system_permissions.php")
            return Implemented("/admin/permissions", "AdminService", "Manage user permissions and roles through the existing admin permissions workflow.");
        if (href is "system_channel_records_sms.php")
            return Implemented("/channels/sms-log", "MessagingService", "Review SMS delivery records through the existing channel log workflow.");
        if (href is "system_channel_records_email.php")
            return Implemented("/channels/email-log", "MessagingService", "Review email delivery records through the existing channel log workflow.");
        if (href is "system_channel_push.php" or "system_channel_records_push.php")
            return Implemented("/channels/push", "MessagingService", "Send and review push notifications through the existing push workflow.");

        if (href.StartsWith("website_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("ecommerce_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("seo_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("social_media_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/tech-enablement/website", "TechEnablementService", "Manage website settings, ecommerce settings, SEO tools, and social media integration records.");
        }

        if (href.StartsWith("organizational_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("system_organization", StringComparison.OrdinalIgnoreCase) ||
            href == "system_template_pdf_signatures.php")
        {
            return Implemented("/tech-enablement/organization", "TechEnablementService", "Manage organizational chart assignments, formal positions, committee assignments, document signatures, and structure cycles.");
        }

        if (href.StartsWith("system_design_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("system_template_images", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("system_template_gifts", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("system_template_greetings", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("system_template_pdf_design", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("system_template_certificates", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/tech-enablement/visual-assets", "TechEnablementService", "Manage dashboard themes, print templates, gift cards, greetings, PDF document designs, and donation certificates.");
        }

        if (href.StartsWith("system_channel_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("channel_whatsapp_", StringComparison.OrdinalIgnoreCase) ||
            href == "system_template_channels_panel.php")
        {
            return Implemented("/tech-enablement/communication", "TechEnablementService", "Manage internal communication templates, WhatsApp templates, push subscribers, local notifications, and channel records.");
        }

        if (href is "system_security_report.php" or "sensitive_pages_report.php")
        {
            return Implemented("/tech-enablement/cybersecurity", "TechEnablementService", "Manage cybersecurity review findings, severity, ownership, mitigation plans, and sensitive permission governance.");
        }

        if (href.StartsWith("ncnp_", StringComparison.OrdinalIgnoreCase) || href == "support_external.php")
        {
            return Implemented("/tech-enablement/ncnp", "TechEnablementService", "Prepare NCNP data for invalid aid records, date and cost updates, ready-to-register records, removed records, external support archive, registered records, and sharing permissions.");
        }

        if (href.StartsWith("system_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/tech-enablement/system", "TechEnablementService", "Manage association information, sections, integrations, warehouses, templates, grading rules, news, variables, administrative requests, internal operations, approval routes, account fields, tutorials, and provider settings.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveVolunteeringImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("volunteering.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href is "volunteer_users_manage.php" or "volunteer_users_database.php" or "volunteer_requests_internal.php" or "volunteer_requests_external.php")
        {
            return Implemented("/volunteering/portal", "VolunteeringService", "Manage volunteer portal accounts and internal/external volunteer requests with approval status tracking.");
        }

        if (href.StartsWith("volunteer_chances_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("volunteer_manage_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/volunteering/opportunities", "VolunteeringService", "Manage volunteer opportunities, procedures, reports, applications, applicants, tasks, and attendance records.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveDocumentationArchiveImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("documentation-archive.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href is "meetings_approval.php")
        {
            return Implemented("/meetings/approvals", "MeetingService", "Approve meeting requests through the existing meeting approval workflow.");
        }

        if (href is "meetings_manage_schedule.php")
        {
            return Implemented("/meetings/scheduled", "MeetingService", "Manage scheduled meetings through the existing meeting scheduling workflow.");
        }

        if (href is "meetings_repeated.php")
        {
            return Implemented("/meetings/repeat-drafts", "MeetingService", "Manage repeated meeting drafts through the existing repeat-draft workflow.");
        }

        if (href is "meetings_finish.php")
        {
            return Implemented("/meetings/finish", "MeetingService", "Finish meetings, decisions, and minutes through the existing meeting closure workflow.");
        }

        if (href is "meetings_mom_approve.php" or "meetings_approve.php" or "meetings_approved.php")
        {
            return Implemented("/meetings/minutes-approval", "MeetingService", "Approve, review, and cancel approvals for archived meeting minutes.");
        }

        if (href is "meetings_calendar.php")
        {
            return Implemented("/meetings/calendar", "MeetingService", "Display meeting calendar data through the existing meeting calendar workflow.");
        }

        if (href is "meetings_admins.php")
        {
            return Implemented("/boards", "BoardService", "Manage board details, memberships, roles, subscription-fee status, supporting-member weights, and cycle renewal.");
        }

        if (href.StartsWith("meetings_database_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/meetings/archive", "MeetingService", "Review approved archived meetings by general, board, and administration meeting scopes.");
        }

        if (href.StartsWith("meetings_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/meetings/rafed", "MeetingService", "Manage Rafed-style meeting creation, databases, reports, delegation requests, and meeting type views.");
        }

        if (href is "corporate_docs_manage.php" || href.StartsWith("archives_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/documentation-archive/archives", "DocumentationArchiveService", "Manage corporate document records and archive categories for general, finance, budget, administration, and secret files.");
        }

        if (href.StartsWith("mail_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/documentation-archive/mail", "DocumentationArchiveService", "Manage outgoing and incoming mail, replies, operations, deletion approvals, databases, and barcode values.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveReportsStatisticsImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("reports-statistics.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href.StartsWith("statistics_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/reports-statistics/statistics", "ReportsStatisticsService", "Generate system statistics dashboards for beneficiaries, relatives, sponsorships, aid requests, projects, tasks, finance, and areas.");
        }

        if (href.StartsWith("report_", StringComparison.OrdinalIgnoreCase) || href == "projects_finance_reports.php")
        {
            return Implemented("/reports-statistics/catalog", "ReportsStatisticsService", "Generate system reports across beneficiaries, HR, aid requests, tasks, sponsorships, supporters, members, projects, media, marketing, mail, website users, payment orders, programs, follow-up, qualification, expenses, income, analytics, and project finance.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveMovementMaintenanceImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("movement-maintenance.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href is "cars_maintenance.php" or "maintenance_request.php")
        {
            return Implemented("/movement-maintenance/maintenance", "MovementMaintenanceService", "Manage vehicle and general maintenance requests, status decisions, costs, vendors, and completion records.");
        }

        if (href.StartsWith("cars_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/movement-maintenance/fleet", "MovementMaintenanceService", "Manage cars, car requests, employee hand-off, employee return, and vehicle movement database records.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveEvaluationFollowUpImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("evaluation-followup.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href.StartsWith("followup_records", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/evaluation-followup/activities", "EvaluationFollowUpService", "Create and list activity follow-up records for general activities, beneficiaries, relatives, aid requests, sponsors, sponsorships, and supporters.");
        }

        if (href.StartsWith("followup_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/evaluation-followup/cases", "EvaluationFollowUpService", "Create, reject, run, complete, approve, manage, and search case follow-up requests and databases.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveInstitutionalExcellenceImplementation(RafedPageSeed pageSeed)
    {
        if (pageSeed.Key.StartsWith("excellence-performance.", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/institutional-excellence/performance", "InstitutionalExcellenceService", "Manage performance-measure reports and statistics with target, actual, period, and achievement tracking.");
        }

        if (!pageSeed.Key.StartsWith("excellence-governance.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href.StartsWith("governance_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("statistics_governance_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/institutional-excellence/governance", "InstitutionalExcellenceService", "Manage governance activation, helper roadmap notes, criteria answers, verification attachments, evaluation sheets, finance indicators, tasks, and statistics.");
        }

        if (href.StartsWith("strategy_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/institutional-excellence/strategy", "InstitutionalExcellenceService", "Manage strategic plans, variables, automated verification values, perspectives, goals, main and sub indicators, owned indicators, project and program indicators, map/tree views, and Vision 2030/sustainability alignment.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveFinancialDevelopmentImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("financial-development.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href.StartsWith("projects_supporters_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/financial-development/supporters", "FinancialDevelopmentService", "Manage supporter accounts, supporter database search, and SMS-ready supporter messaging lists.");
        }

        if (href is "marketing_campaigns.php" or "marketing_campaigns_database.php" or "abandoned_carts.php")
        {
            return Implemented("/financial-development/digital-marketing", "FinancialDevelopmentService", "Manage electronic marketing campaigns, campaign database metrics, and abandoned donation carts.");
        }

        if (href.StartsWith("donations_", StringComparison.OrdinalIgnoreCase) || href == "report_donations.php")
        {
            return Implemented("/financial-development/reports", "FinancialDevelopmentService", "Display donation reports by supporter category, frequency, campaign, urgent campaign, day, category, gift, device, project donations, and smart marketer views.");
        }

        if (href.StartsWith("awqaf", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/financial-development/endowments", "FinancialDevelopmentService", "Manage endowment assets, endowment database, contracts, invoices, and due-soon invoice tracking.");
        }

        if (href.StartsWith("marketing_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/financial-development/fundraising", "FinancialDevelopmentService", "Manage general, project, aid request, program, completed, website donation, external donation, gift, and certificate fundraising flows.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolvePublicRelationsMediaImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("pr-media.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href.StartsWith("media_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/pr-media/relations", "PublicRelationsMediaService", "Manage media partners, events, and visits.");
        }

        if (href.StartsWith("users_website_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/pr-media/website-users", "PublicRelationsMediaService", "Manage website user accounts, database, and login records.");
        }

        if (href.StartsWith("channel_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/pr-media/channels", "PublicRelationsMediaService", "Manage SMS, WhatsApp, email, push templates, lists, subscribers, campaigns, and records.");
        }

        if (href is "website_design.php" or "website_fonts.php" or "website_design_templates.php" or "website_module_custom.php" or "website_sections.php" or "website_short_links.php" or "website_menu.php" or "website_module_slider.php" or "website_module_counter.php")
        {
            return Implemented("/pr-media/design", "PublicRelationsMediaService", "Manage website theme, fonts, templates, custom modules, sections, short links, menu, slider, and counters.");
        }

        if (href.StartsWith("website_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/pr-media/content", "PublicRelationsMediaService", "Manage website content, forms, records, contests, pages, links, popup, and contact requests.");
        }

        return null;
    }

    private static CatalogPageImplementation? TryResolveAccountingImplementation(RafedPageSeed pageSeed)
    {
        if (!pageSeed.Key.StartsWith("accounting.", StringComparison.OrdinalIgnoreCase))
            return null;

        var href = pageSeed.OriginalHref;
        if (href.StartsWith("finance_report", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("statistics_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("finance_budgets", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/accounting/reports", "AccountingService", "Display accounting reports, statistics, statements, and budget deviation views.");
        }

        if (href.StartsWith("revision_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("finance_archive", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/accounting/revision", "AccountingService", "Manage financial revision, approval, rejection, cancellation, and archive records.");
        }

        if (href.StartsWith("expenses_", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("salaries_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/accounting/expenses", "AccountingService", "Manage cash expenses, exchange orders, custody settlement, sponsorship expenses, and salary disbursement.");
        }

        if (href.StartsWith("sanad_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/accounting/receipts", "AccountingService", "Manage receipts, donations, deferred receivables, credits, returns, and received income.");
        }

        if (href.StartsWith("finance_", StringComparison.OrdinalIgnoreCase))
        {
            return Implemented("/accounting/setup", "AccountingService", "Manage accounting settings, bank accounts, chart of accounts, cost centers, ledgers, opening, closing, and renumbering.");
        }

        return null;
    }

    private static CatalogPageImplementation Implemented(string route, string serviceName, string servicePlan) =>
        new(route, serviceName, servicePlan, "Implemented Blazor workflow mapped from Rafed sidebar while preserving the original Arabic label.", SystemPageStatus.Implemented);

    private sealed record CatalogPageImplementation(
        string Route,
        string ServiceName,
        string ServicePlan,
        string UiPlan,
        SystemPageStatus Status);
}
