# Rafed parity gap audit

Audit date: 2026-07-13  
Reference system: `https://www.sarh.org.sa/rafed` (authenticated administrative account)  
Local source of truth: `Application/Service/SystemCatalog/RafedCatalogSeed.cs`

## Executive result

The live Rafed navigation exposes 18 top-level operational domains. The local project contains the same 18 domains and all **688** catalog service links from Rafed. The live `index.php` dashboard is intentionally represented by the local main dashboard and is not one of the 688 catalog services.

There is therefore no navigation-domain gap. The parity gap is functional depth: **638 catalog entries are explicitly marked `Planned`** in the source catalog and require full service-level parity. The remaining 50 entries are explicitly marked implemented; additional entries may be routed to shared workflows by the catalog resolver, but should not be considered full field/action parity until reviewed against their original Rafed page.

## Audit interpretation

- **Implemented** means a local workflow and route exist.
- **Planned** means the link opens a labelled Rafed planned shell. It is discoverable, but it does not yet reproduce the original business workflow.
- A shared local route is useful for navigation parity but does not by itself prove that all original fields, validations, filters, actions, exports, and permission rules are implemented.

## Gap inventory by Rafed domain

| Domain | Planned pages | Missing service families / parts |
|---|---:|---|
| الإدارة الإشرافية والتنفيذية | 21 | Institutional-file map, smart foundation assistant, aid-committee credit and decision forms, approval queues, payment-authorisation registers, cancelled alerts, meeting decisions, and decision-task follow-up. |
| التميز المؤسسي – مقياس الأداء | 2 | Full performance-measure scorecards, target-versus-actual drilldowns, owner follow-up, and measurement export. |
| المكتب الإلكتروني | 49 | Employee self-services, mail preference and archive detail, work calendar, reminders, staff questions/warnings, administrative demands, approvals, and office reporting registers. |
| إدارة الأعضاء المشاركين | 0 | Core catalog entries are mapped; remaining work is detailed field/action parity and history/export parity. |
| حسابات المستفيدين | 40 | Account profiles, guardian records, update operations, entity profile operations, document/image attachments, service history, and record-level approval actions. |
| التميز المؤسسي – الحوكمة والخطة | 24 | Governance roadmap, criteria evidence cycle, financial indicators, strategic-plan hierarchy, perspectives/goals/indicators, and detailed governance reports. |
| البرامج والمشاريع والتصاميم | 94 | The largest programme gap: initiation, planning, schedules, attendance, execution, monitoring, closure, suppliers, proposals, approvals, manager dashboards, programme procedures, reports, and qualification projects. |
| خدمات المستفيدين | 46 | Aid-request routes, payment orders, sponsorship processing, entity requests, coupon lifecycle, external-service flows, and recipient-level tracking. |
| العلاقات العامة والإعلام | 46 | Media relations, partner/visit/event operations, website account/content/design workflows, external-contact operations, and communications reporting. |
| المحاسبة | 101 | The largest overall gap: chart/setup, donations, deferred claims, receipt/expense vouchers, payroll disbursement, financial review, budgets, archives, reconciliations, approvals, and formal accounting reports. |
| التقييم والمتابعة | 18 | Case/activity follow-up, evaluation forms, action plans, progress review, closure evidence, and escalation reporting. |
| الحركة والصيانة | 7 | Vehicle movement, maintenance requests, maintenance approvals, fleet history, costs, and maintenance reporting. |
| التقارير والإحصائيات | 33 | Rafed-specific report definitions, parameter forms, archived report files, printable templates, and source-by-source report fidelity. |
| التوثيق والمستندات | 37 | Meeting variants and archives, agenda/minute templates, delegation workflow, archive classes, incoming/outgoing mail operations, barcodes, and records reports. |
| الموارد المالية والتنمية والتسويق والأوقاف | 31 | Supporter accounts, fundraising operations, digital marketing campaigns, donation reporting, endowment administration, and contribution reconciliation. |
| الموارد البشرية | 29 | Employee account details, HR service requests, attendance configuration, leave/discipline/safety/recruitment processes, letters, payroll decisions, and personnel reports. |
| التمكين التقني | 49 | System configuration, website administration, organization structure, visual-asset management, communication channels, cyber-security monitoring, NCP data preparation, and data-sharing controls. |
| التطوع | 11 | Volunteer accounts, internal/external applications, opportunity procedures, applications, applicants, task allocation, attendance, and opportunity reports. |

## Cross-cutting parity gaps

These parts recur across the planned services and should be delivered as reusable platform capabilities before completing individual pages:

1. **Attachments:** actual upload/storage, virus/type/size policy, secure download, attachment-to-entity links, versioning, and retention.
2. **Exports and print templates:** formal PDF/XLSX templates, Arabic RTL print layouts, signatures/stamps where relevant, and generated-file archiving.
3. **Auditability:** entity timeline, before/after diffs, actor/IP context, and a filtered audit view from each major record page.
4. **Permissions:** route, API, control, and navigation visibility driven from the same role/permission policy.
5. **Search and filtering:** saved filters, date/status/owner scopes, server-side paging, and export of the filtered result set.
6. **Workflow controls:** state-transition validation, approval delegation, deadlines/escalation, rejection reasons, and notification delivery tracking.
7. **Reporting:** operational KPIs, drilldowns, scheduled reporting, and report archive retention.

## Exact source inventory

The complete 688-page inventory, including the exact Arabic label, original Rafed PHP route, target local route, and planned/implemented declaration, is maintained in:

`Application/Service/SystemCatalog/RafedCatalogSeed.cs`

This file is the canonical detailed per-service inventory. It is preferable to manually duplicated lists because it is also the runtime seed used by the sidebar/catalog.
