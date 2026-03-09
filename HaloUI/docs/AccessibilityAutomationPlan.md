# Accessibility Automation Plan

Phase 3 requires automated coverage for accessibility regressions. This plan outlines the tooling stack and rollout steps.

## Objectives
- Enforce WCAG 2.2 AA across key components and flows.
- Combine automated (axe, Playwright) and manual (screen reader) checks with documented evidence.
- Integrate results into CI so regressions block merges and feed KPI reporting.

## Tooling Stack
1. **Playwright** (TypeScript)
   - Render Blazor components via sample host app.
   - Drive keyboard interactions, focus management, and state transitions.
2. **axe-core** (Playwright accessibility plugin)
   - Run axe scans after each scenario to capture violations.
3. **Snapshot/VRT** (optional) – Chromatic or Playwright trace viewer for visual regressions tied to accessibility states.
4. **Reporting**
   - Export JSON + HTML per scenario.
   - Attach artifacts to CI run and link in PR comments.

## Coverage Plan
| Component Group | Scenarios | Priority |
|-----------------|-----------|----------|
| Buttons & Labels | Default/hover/disabled, contrast | High |
| Inputs & Select | Focus, validation errors, helper text, disabled | High |
| Tabs & Navigation | Keyboard roving tabindex, aria-controls | High |
| Dialogs & Snackbar | Focus trap, aria-modal, announcements | High |
| Tables & TreeView | Keyboard navigation, row selection, virtualised data | Medium |

## Rollout Steps
1. **Spike**: Create Playwright test harness pointing to sample page hosting Halo components (Week 1). _Status_: `HaloUI.DemoHost` Blazor server renders real Halo components; Playwright config spins it up automatically.
2. **MVP Suite**: Cover Buttons, Inputs, Tabs with axe assertions + screenshots (Week 2–3). _In progress_ (buttons, text fields, select combobox, tabs, and sample dialog coverage implemented).
3. **CI Job**: Add an AppVeyor accessibility step running Playwright headless and uploading axe reports (Week 3). _Status_: pending CI lane wiring in `appveyor.yml`.
4. **Expand Coverage**: Add dialogs, tables, snackbar; record manual SR expectations (Week 4+). _Snackbar live-region (`tests/accessibility/tests/snackbar.spec.ts`), dialog focus-trap + escape regressions (`tests/accessibility/tests/dialog.spec.ts`), and table semantics (`tests/accessibility/tests/table.spec.ts`) automated; tree/table virtualization scenarios remain._
5. **Documentation**: Publish runbook in `docs/AccessibilityAutomationPlan.md` (this file) and link from `docs/ContributionGuide.md`.

## Metrics & Escalation
- Violations per run tracked in monthly KPIs.
- Failures automatically block PRs; maintain suppressions list for known issues (with expiry).
- Critical regressions logged as risks in `docs/Phase0-Findings.md`.

## Dependencies
- Sample host: `HaloUI.DemoHost` (Blazor Server) referenced via Playwright `webServer` config.
- Node tooling already present in CI (used for npm ci in server build).
- Accessibility lead to own manual SR verification schedule.

---

_Revise this plan as the Playwright suite matures; future ADRs will capture major tooling choices._
