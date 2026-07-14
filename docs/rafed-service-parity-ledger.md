# Rafed service parity ledger

`rafed-service-parity-ledger.csv` is the implementation artifact for step 1 of the live comparison plan. It contains one row for each of the 688 services seeded in `RafedCatalogSeed.cs`.

The generator deliberately writes `ParityStatus = Partial` for every row. A local route or a catalog status of `Implemented` is only mapping evidence; it must not certify a service contract.

## Refreshing the ledger

Run the following from the repository root after the catalog or captured Rafed metadata changes:

```powershell
.\tools\New-RafedParityLedger.ps1
```

To use a newly collected, read-only metadata export, supply it explicitly:

```powershell
.\tools\New-RafedParityLedger.ps1 -MetadataPath .\rafed-page-metadata.json
```

The script fails if it cannot extract exactly 688 unique seed services. It uses the sidebar inventory for every service and imports structural fields, forms, buttons, and table counts only when metadata exists. It does not store page HTML, authentication material, cookies, or operational record values.

## How to complete a row

For the selected service, record the observed original fields, actions, state transitions, validations, list columns/filters, exports, and permissions. Map its local route, API, entity, migration, and automated tests. Keep the row `Partial` until the acceptance checklist has evidence for all applicable behavior, including permission enforcement, Arabic RTL/mobile behavior, audit history, attachments, and filtered exports.

When the contract is proven, change the row to `Complete` and link its acceptance test or evidence in `TestCoverage` and `AcceptanceChecklist`. The catalog seed may then be changed from `Planned`; the ledger remains the source of parity certification.
