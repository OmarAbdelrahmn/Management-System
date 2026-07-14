# Rafed live comparison: implementation update and remaining evidence

Audit date: 2026-07-14  
Reviewed baseline: `docs/rafed-live-comparison-gaps.md`

## Current result

The locally actionable platform gaps found in the comparison were substantially implemented. The application builds without warnings and all 164 tests in the full suite pass, including the all-report dataset regression test.

This does **not** certify all 688 Rafed services as contract-complete. The repository still lacks original-page evidence and completed acceptance records for those services. Route coverage and service-contract parity remain different measures.

## Implemented in this follow-up

### Authorization

- Added database-backed exact and prefix permission policies with Admin bypass.
- Replaced all broad controller role attributes with exact or prefix catalog permissions across the API surface.
- Added exact report-key authorization for report generation, export, and archived downloads.
- Added exact task permissions for list, create, update, complete, redirect, remove, restore, activity, and comment endpoints.
- Added permission-denial audit events for direct catalog route denials, including actor and request IP, with duplicate suppression.
- Current static counts: 83 exact permission attributes, 22 permission-prefix attributes, 0 controller role attributes, and 150 role-based Razor page attributes.

### Attachments

- Added a permission access endpoint and permission-aware attachment controls.
- Removed the fixed controller upload limit and aligned multipart limits with `Attachments:MaximumSizeBytes`.
- Corrected ledger view/manage permission behavior and tested view-only versus manage access.
- Expanded supported attachment entities and record-page adoption from 4 to 10 pages/entities, including members, employees, projects, aid requests, follow-up cases, and executive decisions.
- Kept preview, versioning, unlink, soft delete, retention, purge, and attachment audit behavior in the shared platform.

### Reports and saved views

- Replaced all 21 placeholder report datasets with filtered domain queries.
- All 36 default report/statistic definitions now resolve to a registered dataset; an automated test iterates all definitions and verifies that no fallback placeholder is emitted.
- Added per-report permission enforcement to generate/export/download boundaries.
- Added a reusable saved-query-view component and integrated it into beneficiary and employee operational lists.

### Audit timeline

- Added safe field-level before/after diff rendering to `EntityTimeline`.
- Increased timeline visibility through attachment panels on 10 major record pages.
- Added route permission-denial auditing so direct denied navigation is captured centrally.

### Shared approvals

- Added an idempotent, route-driven `EnsureApprovalRequestForEntityAsync` bridge.
- The bridge creates one pending shared approval when an active route exists and safely leaves the domain operation unchanged when no route is configured.
- Connected new beneficiary aid requests, employee leave requests, program approvals, and executive approval requests to the shared engine.
- Added configured-route, duplicate-prevention, and unconfigured-route tests.

## Remaining work that cannot be declared complete from this repository alone

### P0 — Capture and certify the 688 original service contracts

The parity ledger still needs original Rafed evidence for each service: fields, actions, states, validations, tables, filters, exports, permission rules, and responsive/RTL behavior. Each row then needs links to its local API/entity/migration/tests and completed acceptance results.

The current route resolver maps all 688 entries to local routes, but that is navigation coverage—not proof that 688 distinct contracts match the live system.

### P0 — Finish authorization migration at page/control level

The API controller role migration is complete, but 150 Razor role attributes remain. Shared pages also need control-level permission checks wherever multiple Rafed services share one workspace. Granted, denied, revoked, and Admin cases must ultimately be recorded per ledger contract.

### P1 — Complete cross-domain adoption

- Attachments and visible timelines are present on 10 major pages, not every record type that may require them.
- Saved views are integrated on 2 operational lists; server paging, active-filter exports, and saved views are not yet standardized across every applicable list.
- Four domains create shared approval requests. Accounting, documentation, maintenance, and other approval-bearing records still require route integration or evidence of equivalent behavior.
- A final shared approval decision is not yet a generic callback into every referenced domain status; each domain needs an explicit, validated transition adapter before that can be safely automated.

### P1 — Add report scheduling and formal outputs

Report scheduling/delivery and report-specific Arabic forms, signatures, stamps, and approval templates are not implemented. Current PDF/XLSX output is generic and should not be represented as a certified copy of each original Rafed form without original templates.

### P1 — Produce release-grade verification

Still required:

- Automated browser smoke coverage for all 688 navigation targets and primary actions.
- RTL, mobile, accessibility, keyboard, and visual-regression checks.
- Large-dataset performance evidence and filtered-export verification.
- Migration upgrade/rollback, production job configuration, backup, and restore evidence.
- Production verification that recurring attachment purge and approval escalation jobs are enabled.

## Verification completed

- `dotnet build "Express Service/Express Service.csproj" --no-restore --nologo`: passed, 0 warnings, 0 errors.
- Full automated suite: 164 passed, 0 failed, 0 skipped.
- Focused report suite after the all-definition dataset test: 8 passed, 0 failed.
- Focused permission, catalog, attachment, report, and workflow suite: 47 passed, 0 failed.

## Accurate completion statement

The concrete reusable-platform defects identified by the audit have been implemented or materially reduced. The remaining document is now primarily a parity-certification and full-adoption backlog. The accurate status remains: **688 services are route-mapped; 0 services can be called parity-certified until original Rafed evidence and per-service acceptance results are recorded.**
