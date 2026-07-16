# Login design QA

- Source visual truth: `docs/login-design-qa/source-landing-desktop.png`
- Implementation: `docs/login-design-qa/login-after-desktop.png`
- Desktop viewport and state: 1440 × 900, initial login state
- Mobile viewport and state: 390 × 844, initial login state
- Full-view comparison evidence: `docs/login-design-qa/login-comparison-desktop.png`
- Mobile evidence: `docs/login-design-qa/login-after-mobile.png`
- Focused-region comparison: not required; the desktop comparison retains the full 1440 × 900 source and implementation at native resolution, with the complete brand header, headline, form, controls, and footer visible.

## Design intent

The landing page is the visual-language target rather than a pixel-identical login mockup. The login intentionally reuses its Arabic RTL typography hierarchy, deep-green and warm-gold palette, pale organic background, compact brand mark, border treatment, button styling, spacing rhythm, and restrained elevation while replacing the landing hero product art with the employee sign-in form.

## Comparison findings

- P2 accessibility: the compact mobile home control measured 42 px high. Fixed by increasing its minimum height to 44 px.
- Typography: heading weight, green emphasis, Arabic line height, and supporting-copy density align with the landing page. No clipping or broken wrapping was found.
- Spacing and layout: desktop uses a balanced two-column story/form composition; mobile collapses to one column with consistent margins and a scrollable form. No horizontal overflow was found.
- Colors and surfaces: green, gold, off-white, subtle borders, rounded corners, and card shadow map to the landing page tokens without introducing a generic dashboard style.
- Assets and icons: the existing Font Awesome family is used consistently. The login does not require the landing hero image, and no placeholder or custom SVG illustration was substituted.
- Copy: the company name, employee-portal context, security note, labels, error copy, and copyright are coherent and complete in Arabic.

## Interaction and accessibility checks

- Dashboard-to-login redirect verified with the return URL preserved.
- Email and password fields accepted input and retained the expected accessible labels.
- Error state at `/login?error=1` exposed a semantic live alert with actionable Arabic copy.
- Form action and method verified as `POST /ui/login`.
- Email, password, and submit controls measure 50 px high at 390 px width.
- Mobile document width does not exceed the viewport.
- Final browser console check returned no errors or warnings.

## Comparison history

1. Baseline login used an unrelated navy/white legacy card and old product naming.
2. Rebuilt the page with the landing page's brand language, complete company naming, responsive layout, form states, and accessible labels.
3. Corrected the mobile home-control tap target and repeated responsive/browser checks.

final result: passed
