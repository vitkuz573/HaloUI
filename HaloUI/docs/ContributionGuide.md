# HaloUI Contribution Guide

Use this guide when proposing, implementing, or reviewing changes to the HaloUI. It complements the Definition of Ready/Done and component/token checklists.

## 1. Before You Start
- Review `docs/Charter.md`, `ROADMAP.md`, and `docs/Phase0-Findings.md` to understand priorities.
- Confirm the work item satisfies the Definition of Ready (`docs/DefinitionOfReadyDone.md`).
- For large changes, draft an RFC (`docs/rfc/`) and link it to issues/PRs.
- Align on ownership: assign a design partner, accessibility reviewer, and engineering lead.

## 2. Development Standards
- Follow `.editorconfig` and `STYLEGUIDE.md` (4-space indent, Allman braces, nullable enabled).
- Prefer strongly typed tokens over raw CSS values; run `TokenValidator` for new token sets (see `docs/TokenValidation.md`).
- Keep components accessible: keyboard support, focus management, ARIA annotations, reduced-motion respect.
- Shared logic belongs in `Abstractions/` or `Services/` to minimise duplication.
- For diagnostics/services, ensure DI registration and lifecycle expectations are documented.

## 3. Testing Expectations
- Unit/bUnit tests for behaviour changes (`HaloUI.Tests`).
- Token completeness/validation tests updated when manifest changes.
- Interaction/visual tests (Playwright) for critical flows when available.
- Accessibility automation (axe/Playwright per `docs/AccessibilityAutomationPlan.md`) plus manual SR walkthroughs for UI-impacting work.
- New public components must be registered in `HaloUI.Tests/AccessibilityCoverageContractTests.cs`; CI fails if coverage classification/evidence is missing.
- New public components must be registered in `HaloUI.Tests/ResponsiveCoverageContractTests.cs`; CI fails if responsive classification is missing.
- Capture coverage impact or rationale when tests are not feasible.

## 4. Documentation
- Update README/Storybook/docs for new components, states, or tokens.
- Record decisions through ADRs (`docs/adr/`) when architecture shifts occur.
- Add release-note entries using `docs/releases/ReleaseNotes-template.md`.
- Provide migration notes for breaking changes (and mark them in release docs).

## 5. Pull Requests
- Link issues, RFCs, and ADRs.
- Attach completed checklists (`docs/checklists/Component.md` / `docs/checklists/Token.md`).
- Include screenshots/GIFs for UI changes (light/dark, states, high-contrast).
- Note testing evidence (commands, logs) and accessibility verification.
- Request reviews from product, design, accessibility, and tech leads as needed.

## 6. Onboarding & Support
- New contributors should review the roadmap and governance docs, then pair on their first change.
- Use #haloui for questions; schedule office hours for complex integrations.
- Document lessons learned in `docs/Phase0-Findings.md` or follow-up ADRs/risk register entries.

---

_Questions? Ping the governance board or open a discussion thread in the repository._
