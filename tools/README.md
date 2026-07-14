# Tools

## New-RafedParityLedger.ps1

Generates the 688-service parity ledger from the canonical catalog seed, sidebar inventory, and optional read-only page metadata. Every generated row starts as `Partial`; an `Implemented` route is not parity certification.

```powershell
.\tools\New-RafedParityLedger.ps1
```

Use a refreshed structural metadata export when available:

```powershell
.\tools\New-RafedParityLedger.ps1 -MetadataPath .\rafed-page-metadata.json
```

## Export-RafedPageMetadata.ps1

Read-only Rafed audit helper for refreshing page metadata after a live sidebar export.

- Uses `RAFED_USERNAME` and `RAFED_PASSWORD` environment variables, or prompts interactively.
- Performs login, then only `GET` requests for page inspection.
- Writes structural metadata only: title, form count/actions, input names/types, table count, and button labels.
- Does not write cookies, credentials, HTML, field values, or operational record text.

Example:

```powershell
$env:RAFED_USERNAME = "user"
$env:RAFED_PASSWORD = "password"
.\tools\Export-RafedPageMetadata.ps1 -LinksPath .\rafed-links.json -OutputPath .\rafed-page-metadata.json -Limit 25
```
