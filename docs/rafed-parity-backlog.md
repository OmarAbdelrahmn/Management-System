# Rafed parity implementation backlog

This backlog converts the audit into deliverable work packages. Work is ordered by dependency and operational value, not by menu order.

## Priority 0 — platform foundations

Complete these first because they remove repeated work across every domain.

| Work package | Required outcome | Accept when |
|---|---|---|
| Attachment platform | Upload, secure storage, metadata, entity links, download authorization, preview, deletion/retention, and audit events. | A record in HR, beneficiaries, meetings, and accounting can use the same attachment workflow. |
| Permission visibility | Shared permission checks for APIs, routes, sidebar items, buttons, and exports. | A user cannot discover or invoke an action they are not permitted to use. |
| Reporting platform | PDF/XLSX/CSV/TSV/JSON exports, RTL print layouts, archived files, and report-run audit records. | A filtered operational report can be exported and printed consistently. |
| Audit timeline | Entity-level events and change diffs with filters. | A user can see who changed a record, when, and what changed. |
| Query platform | Server paging, sorting, advanced filters, and saved views. | Large databases remain usable and exports respect the active filters. |

## Priority 1 — highest-volume Rafed domains

### Accounting — 101 planned pages

Implement in this sequence:

1. Chart of accounts, accounting settings, fiscal periods, and opening balances.
2. Receipts, expenses, donations, deferred claims, and approval flows.
3. Payroll disbursement/export, reconciliation, and financial review.
4. Budget planning, commitments, variance analysis, and budget approvals.
5. Formal accounting reports, archive, print templates, and spreadsheet/PDF export.

Critical parity parts: voucher numbering, posting/unposting controls, immutable audit trail, attachments for proofs, bank references, period locks, and approval segregation.

### Programs, projects, and designs — 94 planned pages

Implement in this sequence:

1. Programme/project initiation and proposals.
2. Planning: tasks, schedules, resources, budgets, risks, and approvals.
3. Execution: attendance, beneficiary participation, suppliers, custody, income, expenses, and evidence.
4. Monitoring: KPIs, progress, issues, decisions, and escalations.
5. Closure/evaluation: outcomes, surveys, certificates, reports, and archive.

Critical parity parts: project state machine, programme manager dashboard, dependencies, budget-vs-actual metrics, and printable project reports.

### Electronic office and technical enablement — 98 planned pages combined

Implement office communication/requests before technical administration screens so the organization has operational demand and approval data.

1. Internal mail, drafts, templates, archive, routing, and delivery status.
2. Office requests and employee self-service workflows.
3. System/website/organization settings with approval and audit requirements.
4. Communications, cybersecurity records, NCP preparation, and data-sharing controls.

## Priority 2 — beneficiary-facing operations

### Beneficiary accounts — 40 planned pages

- Full beneficiary, guardian, and entity account record screens.
- Identity/documents/images, update requests, eligibility evidence, and history.
- Scoped search, privacy controls, approval, and export.

### Beneficiary services — 46 planned pages

- Aid request intake, social research, decisions, payment orders, sponsorships, entity supports, and coupons.
- Recipient and payment reconciliation; cancellation/rejection reasons; service history.

### Human resources — 29 planned pages

- Employee profiles, recruitment-to-employee conversion, attendance configuration, leave, discipline, safety, letters, payroll decisions, and personnel reports.

## Priority 3 — governance and oversight

### Governance, boards, and documentation — 61 planned pages combined

- Governance criteria, evidence, roadmap, strategic plan, indicators, task follow-up, and report packs.
- Meeting type variants, agendas, approvals, delegated attendance, minutes, decisions, and archives.
- Board membership lifecycle, fee status, role history, cycle renewal, quorum, and weighted voting evidence.

The local project already contains foundational board management and weighted voting. Remaining work should focus on documentary evidence, delegated approvals, formal minutes/decision templates, and audit-grade reports.

### Reports, evaluation, movement, volunteering — 69 planned pages combined

- Rafed-specific report parameter forms and archives.
- Case/activity monitoring and evaluation cycles.
- Fleet/movement/maintenance costs and approvals.
- Volunteer registration, applications, opportunity execution, attendance, and reporting.

## Priority 4 — communications and financial development

### PR/media — 46 planned pages

- Contacts, media partners, events, visits, content, website users, web design/content approval, and communications analytics.

### Financial development — 31 planned pages

- Supporter lifecycle, donation campaigns, digital marketing, endowments, contribution reconciliation, and donor reports.

## Definition of done for every Rafed service

Do not mark a page as implemented merely because it has a route. It is complete only when all applicable items are true:

- Exact Rafed Arabic label and catalog placement are retained.
- Role/permission policy is enforced in API, route, navigation, and controls.
- Record creation, editing, viewing, cancellation/archive, and validation behavior are operational.
- Search, filter, paging, empty state, and error messages are provided.
- Attachments, audit history, and approvals are available where the domain requires them.
- Export/print requirements are available.
- RTL and mobile layouts are verified with live authenticated roles.
- The service has a functional local route and is no longer marked `Planned` in the catalog.
