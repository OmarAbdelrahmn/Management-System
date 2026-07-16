# Mobile frontend audit

## Scope

Combined responsive UX and accessibility review of the Blazor frontend at phone and tablet portrait widths. The source scan covered 167 page components, including 124 pages using shared form grids and 134 pages containing data tables.

## User goal and accessibility target

Enable staff to navigate, search, read, and complete common management tasks on narrow screens without page-level horizontal overflow, clipped controls, undersized touch targets, or multi-column forms that become unusable.

## Captured steps

1. **Landing page, 390px before the shared responsive pass — generally healthy.** The main hierarchy and calls to action already reflowed, but the verification established the visual baseline for narrow screens.
2. **Login page, 390px before — needs improvement.** The card fit the viewport, but text inputs and the submit action used desktop-sized control heights.
3. **Login page, 320px after — healthy.** Inputs and the submit action are 44px high, the checkbox is 20px, the card fits within the viewport, and document scroll width equals viewport width.
4. **Landing page, 320px after — healthy.** Navigation, headline, calls to action, and hero visual remain readable with no horizontal page overflow.
5. **Landing page, 768px after — healthy.** The hero and service layout stack at tablet portrait width, avoiding a compressed two-column presentation.
6. **Authenticated application shell and data pages — structurally improved; screenshot verification blocked.** The local project has no configured test account. These pages were reviewed from their shared layout, form, table, tab, action-bar, and navigation source patterns without weakening authentication or creating a user in the configured database.

## Changes applied

- Responsive shell breakpoint expanded through tablet portrait and landscape-phone widths.
- Mobile header now has separate navigation and account-action toggles; dropdowns expand within the header instead of being clipped by a horizontal scroller.
- Regular and compact form grids collapse consistently, including a fix for two-column fields creating implicit overflow.
- All mobile inputs use a 44px minimum height and 16px text; checkboxes use a 20px target.
- Action rows wrap, then become full-width stacked actions on narrow phones.
- Tables scroll within their own surface, with stable headers and touch-friendly row actions.
- Tabs remain in one swipeable row; dashboards and split/voting layouts become one column.
- Focus indicators, reduced-motion behavior, safe-area padding, text wrapping, and page-level overflow protection were added.

## Evidence limits

Screenshots confirm the public landing and login experiences at 320px, 390px, and 768px. Full interaction, keyboard, screen-reader, and authenticated-page visual verification still requires a safe local test account. This audit does not claim full WCAG conformance.

## Evidence files

- `01-landing-viewport-before.png`
- `02-login-before.png`
- `03-login-after-320.png`
- `04-landing-after-320.png`
- `05-landing-after-tablet.png`
- `06-login-after-390.png`
