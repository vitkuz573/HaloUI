# RFC-0002: Accessibility Automation Suite (Playwright + axe)

- **Authors**: Codex Agent, @vitkuz573
- **Status**: Draft
- **Created**: 2024-05-29
- **Updated**: 2024-05-29
- **Tracking Issues**: #000 (placeholder)

## 1. Summary
Implement an automated accessibility regression suite leveraging Playwright and axe-core to validate key Halo components across states, aligning with Phase 3 objectives and WCAG 2.2 AA commitments.

## 2. Motivation
- Manual a11y QA does not scale; regressions slip between releases.
- Consumer teams require evidence (reports, artifacts) that Halo components meet accessibility requirements.
- CI enforcement reduces risk and satisfies governance DoD criteria.

### Non-goals
- Replacing manual screen reader testing entirely (remains required for key flows).
- Building a full visual regression infrastructure (considered separately).

## 3. Detailed Design
### Test Harness
- Create `HaloUI.AccessibilityTests` (TypeScript/Playwright).
- Host Halo components via lightweight sample app (Blazor Server) served during tests.
- Each test renders a component story (buttons, inputs, tabs, dialogs, table, snackbar).

### Automation Flow per Scenario
1. Navigate to component page/state.
2. Run keyboard interactions (tab/focus, arrow navigation, toggles).
3. Execute `await axe.analyze()` to capture violations.
4. Capture screenshot + Playwright trace for debugging.

### CI Workflow
- Add an AppVeyor accessibility step with:
  - Checkout repo, install Node dependencies for test app.
  - Build sample host + run Playwright tests headless.
  - Upload axe reports + traces as artifacts.
- Job required on PRs touching Halo (enforced later via path filters).

### Reporting & Documentation
- Link artifacts in PR comments.
- Update `docs/AccessibilityAutomationPlan.md` and `docs/ContributionGuide.md`.

## 4. Alternatives Considered
- **axe CLI only**: insufficient coverage of interactive flows.
- **WebDriver-based suites**: slower and heavier; Playwright offers better DX and trace tooling.

## 5. Risks & Mitigations
- **Flaky tests**: Use stable selectors, deterministic sample data, and retries.
- **Runtime cost**: Scope MVP suite to high-priority components; parallelize in CI.
- **Maintenance**: Assign owner (Accessibility Lead) and add contributions guide.

## 6. Dependencies
- Node 20 environment (already used).
- Sample host app (can reuse existing docs site once built).
- axe-core Playwright integration.

## 7. Rollout Plan
1. Spike harness + sample page (Week 1).
2. Implement MVP coverage (Buttons, Inputs, Tabs) with CI job in soft-check mode (Week 2–3).
3. Enable blocking mode once flakes resolved (Week 4).
4. Expand coverage and add manual SR checklist references (Week 5+).

## 8. Open Questions
- Should we bundle WCAG contrast exports per story?
- How do we manage waivers/suppressions (JSON config vs. issue labels)?
- Where to host the sample app long term (Storybook vs. dedicated site)?

## 9. Appendix
- `docs/AccessibilityAutomationPlan.md`
- `docs/ContributionGuide.md`

---

_Update status to Accepted once the plan is approved and work begins._
