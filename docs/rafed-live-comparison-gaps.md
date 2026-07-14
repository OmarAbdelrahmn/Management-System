# Rafed live comparison: gaps, risks, and implementation plan

Audit date: 2026-07-13  
Reference system: https://www.sarh.org.sa/rafed/ (authenticated review; no records were created or changed)  
Local source inventory: `Application/Service/SystemCatalog/RafedCatalogSeed.cs`

## Executive result

The local project has full **navigation/catalog coverage**, but not full **business-service parity** with Rafed.

| Measure | Result |
|---|---:|
| Rafed version observed | 1.61.1 |
| Live operational domains | 18 |
| Live service links inventoried | 688 |
| Local catalog links | 688 |
| Seed entries explicitly marked `Implemented` | 50 |
| Seed entries explicitly marked `Planned` | 638 |
| Local Blazor pages | 166 |
| Local API controllers | 32 |
| Automated tests | 147 passed / 147 total |

The local catalog resolver also maps 219 exact Rafed PHP URLs to 46 shared local routes, plus additional rule-based mappings. This is useful route consolidation, but it is **not proof of field/action parity**. A shared local route must still implement every applicable original form field, validation, state transition, filter, export, audit record, and permission.

Examples of high consolidation:

| Shared local route | Rafed services consolidated |
|---|---:|
| `/beneficiary-services/sponsorships` | 22 |
| `/programs-projects/ideas` | 15 |
| `/programs-projects/execution` | 13 |
| `/beneficiaries/update-batches` | 13 |
| `/beneficiary-services/aid-requests` | 11 |

## What Rafed does that is deeper than the local implementation

- **Beneficiaries:** detailed identity, guardian, education, employment, address, income, needs, temporary status, warehouse/archive, documents, and eligibility data.
- **Members:** a profile/table with more than 40 operational fields for board decisions, subscriptions, voting eligibility, participation, education, employment, address, and history.
- **Projects:** full charter information including target group, support type, vision, mission, objectives, SWOT, team, schedule, budget, execution, monitoring, and closure.
- **Meetings:** recurrence, quorum, candidates, guests, voting, reminders, approval records, attachments, minutes, and exports.
- **HR:** identity documents, image/signature, qualification, experience, job data, attendance, salary, insurance, and service history.
- **Accounting:** Excel import, multi-operation journal entries, posting controls, review, budgets, archives, reconciliation, and formal financial reports.
- **Permissions:** fine-grained service access; not merely broad role access.

## Service gap inventory

The counts below are source entries still explicitly declared `Planned`. They are service-level gaps, not necessarily a requirement to create 638 separate screens; a well-designed shared workspace may satisfy several services when it preserves all their distinct capabilities.

| Rafed domain | Planned services | Missing services and functional parts |
|---|---:|---|
| الإدارة الإشرافية والتنفيذية | 21 | Institutional-file map, committee credit and decisions, approval queues, payment authorisations, cancelled alerts, meeting decisions, and decision follow-up. |
| التميز المؤسسي - مقياس الأداء | 2 | Scorecards, target-vs-actual drilldown, owners, periods, and exports. |
| المكتب الإلكتروني | 49 | Mail states/templates/archive, calendar, reminders, employee requests, approvals, transactions, and office reporting. |
| إدارة الأعضاء المشاركين | 0 | Routes exist; detailed profile, subscriptions, voting eligibility, participation, history, and export parity remain. |
| حسابات المستفيدين | 40 | Full intake, guardians, entities, family/income/needs, update operations, documents, approvals, and history. |
| التميز المؤسسي - الحوكمة والخطة | 24 | Evidence cycles, evaluation sheets, roadmap, financial indicators, strategic hierarchy, and reports. |
| البرامج والمشاريع والتصاميم | 94 | Charter, proposals, planning, teams, suppliers, attendance, execution, monitoring, surveys, certificates, qualification, and closure. |
| خدمات المستفيدين | 46 | Aid intake/research, approvals, payment orders, sponsorships, entity assistance, coupons, and reconciliation. |
| العلاقات العامة والإعلام | 46 | Contact requests, partners, visits, events, content, website/design approvals, and communications analytics. |
| المحاسبة | 101 | Setup, journals, receipts, expenses, donations, payroll, claims, reviews, budgets, reconciliation, archives, and statements. |
| التقييم والمتابعة | 18 | Cases, activities, evidence, action plans, escalation, closure, and reports. |
| الحركة والصيانة | 7 | Vehicle assignment/return, maintenance requests, approvals, costs, history, and reporting. |
| التقارير والإحصائيات | 33 | Rafed parameter screens, dynamic query conditions/groups, scheduling, archives, XLSX, and formal Arabic templates. |
| التوثيق والمستندات | 37 | Meeting variants, agendas/minutes, delegation, correspondence, barcodes, PDF templates, signatures, and archives. |
| الموارد المالية والتنمية والتسويق والأوقاف | 31 | Supporters, VAT/tax data, fundraising, campaigns, donations, endowments, and reconciliation. |
| الموارد البشرية | 29 | Complete employee profile, attendance, payroll, insurance, leave, discipline, safety, recruitment, letters, and reports. |
| التمكين التقني | 49 | Settings, organisation structure, website administration, visual assets, communication channels, cybersecurity, NCP, and data-sharing controls. |
| التطوع | 11 | Volunteer profiles, applications, opportunities, tasks, attendance, documents, and reporting. |

## Cross-cutting gaps and risks

### 1. Permission enforcement is incomplete

- The project has a database-backed permission evaluator and a permission-aware sidebar.
- Only 7 `[RequirePermission]` attributes are currently present, all on the attachments controller.
- Controllers still contain 73 role-based authorization attributes.
- 164 protected Blazor pages use `[Authorize]` or role checks; none uses a catalog permission policy.
- Direct-route protection, header shortcuts, navigation search, tabs, buttons, exports, and most API actions are not yet consistently permission-driven.
- Permission denials and permission-management changes are not consistently written to the audit log with request IP context.

### 2. Attachment platform is only partially adopted

The new private, scanned, versioned attachment service is a good foundation, but it is not yet complete:

- It is embedded on only four pages: accounting ledger, beneficiary update, leave requests, and scheduled meetings.
- It uses generic `system.attachments.view/manage` permissions instead of authorizing against the linked entity's capability.
- The UI lacks complete version-history, unlink, preview, and permission-aware control behavior.
- Size and retention values are hard-coded instead of options/configuration.
- Scan attempts and failures need complete audit coverage.

### 3. Legacy file endpoints are a security risk

The legacy Admin file API and manual File Assets page still coexist with the new attachment service.

- API responses and the old UI expose `StoragePath`.
- The old upload endpoint writes the file before the metadata save returns an invalid-request result, which can leave an unscanned orphan file.
- The legacy API permits metadata-oriented file operations that bypass the secure upload/version/scanner flow.
- The legacy endpoints and page should be retired or redirected to the secure attachment service before further service development.

### 4. Reporting, workflow, and operational UX are incomplete

- Reporting supports CSV, TSV, JSON, and PDF, but not XLSX, reusable RTL print templates, signatures/stamps, scheduled delivery, or generated-file archives.
- Saved-query infrastructure exists but is not consistently integrated into operational lists.
- Server-side paging, filtering, saved views, and filtered exports are inconsistent.
- Workflow rules need standard delegation, rejection reasons, deadlines, escalation, notifications, and delivery tracking.
- Header search is not functional, notification counts are static, and header shortcuts are not permission-filtered.

### 5. Audit coverage needs to become record-centric

- Before/after JSON is available for some audit events.
- Major records need a visible entity timeline with actor, time, request/IP context, status transition, attachment event, and export event.
- Financial and approval actions need immutable, segregated audit trails.

## Ten-step implementation plan

1. **Create a 688-service parity ledger.** For every Rafed service, record the original fields, actions, states, validations, tables, filters, exports, permissions, local route/API/entity, migration, and tests. Mark a consolidated route `Partial` until the contract is verified.

2. **Finish unified authorization.** Replace broad controller/page roles with action-specific catalog policies; protect direct routes, APIs, controls, tabs, downloads, exports, header shortcuts, and search results. Audit denials and permission changes with actor and IP.

3. **Finish and standardize attachments.** Retire the legacy file API/page, configure scanner/size/retention options, migrate existing records to versions, authorize by linked entity, and add secure upload/list/download/history/replace/unlink/delete/purge controls across all applicable domains.

4. **Complete shared operational infrastructure.** Standardize audit timelines, state-transition validation, approvals, delegation, rejection reasons, deadlines, escalation, notifications, server paging, saved views, XLSX/PDF exports, Arabic RTL print templates, and report archives.

5. **Deliver accounting parity.** Build the 101-service accounting contract: periods, opening balances, journals, vouchers, posting locks, claims, donations, payroll, reconciliation, financial review, budgets, archives, statements, imports, and exports.

6. **Deliver programs and projects parity.** Complete proposal-to-closure workflows: charter/SWOT, teams, schedules, budgets, suppliers, registrations, attendance, execution finance, KPIs, risk, surveys, certificates, and qualification projects.

7. **Deliver beneficiary ecosystem parity.** Complete beneficiary/guardian/entity records, family/income/needs/eligibility, update operations, social research, aid decisions, payment orders, sponsorships, coupons, documents, and reconciliation.

8. **Deliver employee and field operations.** Complete electronic office, HR, vehicle maintenance, and volunteering: self-service, mail/transactions, personnel lifecycle, payroll/attendance/safety, fleet lifecycle, volunteer opportunities, tasks, and attendance.

9. **Deliver governance and external operations.** Complete executive supervision, performance, governance/strategy, members, documentation, evaluation, reports, PR/media, financial development, technical administration, cybersecurity, NCP, and data-sharing services.

10. **Certify parity and release safely.** Verify all 688 routes and service contracts with granted/denied/revoked/Admin authorization cases, Arabic RTL/mobile UI, audit trails, private attachments, exports, migrations, performance, backup, and rollback. Remove `Planned` only after the service acceptance checklist passes.

## Definition of done for each service

A Rafed service is complete only when all applicable criteria are met:

- The Arabic label, module placement, and required business behavior are preserved.
- API, direct route, navigation item, buttons, tabs, attachments, downloads, and exports enforce the same permission source.
- Create, view, edit, approval/rejection/cancellation/archive, validation, and state history work.
- Lists support search, filter, paging, empty/error states, saved views where useful, and filtered export.
- Attachments, audit timeline, approvals, notifications, and document templates are available where required.
- Arabic RTL, mobile behavior, accessibility, migration, and automated tests are verified.
- The catalog entry is no longer `Planned` only after its acceptance contract passes.

## Related local files

- `Application/Service/SystemCatalog/RafedCatalogSeed.cs` - canonical 688-service inventory.
- `Application/Service/SystemCatalog/SystemCatalogService.cs` - shared-route resolver and navigation permission filtering.
- `Application/Service/Attachments/AttachmentService.cs` - secure attachment foundation.
- `Express Service/Controllers/AdminController.cs` - legacy file API requiring retirement.
- `docs/rafed-gap-audit.md` - earlier inventory.
- `docs/rafed-parity-backlog.md` - earlier implementation backlog.
