# HaloUI Roadmap

## Guiding Principles
- Treat the HaloUI as a product: clear backlog, owners, release cadence.
- Maintain WCAG 2.2 AA, multi-brand, multi-device guarantees across components.
- Keep tokens as the single source of truth; changes flow through automated validation.
- Prioritise developer experience and documentation to accelerate host teams.

## Phase Overview
- **Phase 0 – Baseline Readiness (2 weeks)**: Audit current state, expose risks, align on Definition of Ready/Done.
- **Phase 1 – Governance & Process (2 weeks)**: Formalise lifecycle, versioning, and review discipline.
- **Phase 2 – Tokens & Theming (3 weeks)**: Harden token pipeline, brand management, and regression coverage.
- **Phase 3 – Components, UX & Accessibility (4–6 weeks)**: Close organization-grade gaps across controls, states, and accessibility.
- **Phase 4 – Developer Experience & Distribution (3 weeks)**: Build documentation, tooling, and release automation.
- **Phase 5 – Support & Continuous Evolution (ongoing)**: Establish telemetry, feedback loops, and quarterly planning.

Owners and timelines can be refined once staffing is confirmed. Each phase assumes a cross-functional squad (design, frontend, accessibility, QA, DevOps) plus a tech lead.

---

## Phase 0 – Baseline Readiness
- **Objectives**
  - Catalog components, tokens, services, and tests; classify maturity (Stable/In Progress/Deprecated).
  - Identify risks in accessibility, performance, testing, and developer workflow.
  - Produce a shared backlog of technical debt and high-priority gaps.
- **Exit Criteria**
  - Inventory spreadsheet or project board seeded with all assets and status tags.
  - Findings report with prioritised risk list and recommended sequencing.
  - Definition of Ready/Done agreed for future work (including accessibility and test expectations).
- **Workstreams**
  - Component inventory sweep (`Components/`, `Accessibility/`, `Services/`).
  - Token manifest & generator review (`Theme/Tokens`, analyzers, CI hooks).
  - Test coverage analysis (`HaloUI.Tests`), including snapshot viability.
  - Toolchain assessment (build, analyzers, Storybook gap).

## Phase 1 – Governance & Process
- **Objectives**
  - Define product charter, supported scenarios, and release policy (SemVer + RFC for breaking changes).
  - Introduce formal component lifecycle and review checkpoints.
  - Create governance artefacts (templates, ADRs, contribution guidelines).
- **Exit Criteria**
  - Charter document approved; governance section added to repository docs.
  - Issue/PR templates capturing design review, accessibility, and test sign-off.
  - ADR repository with first entries (theme evolution, component acceptance).
- **Workstreams**
  - Draft charter and lifecycle flow diagram; review with stakeholders.
  - Publish contribution and review checklists (component proposal, design review, QA).
  - Set up RFC process (e.g., `/rfcs` folder with template, triage schedule).
  - Align release cadence and communication (release notes, support window).
 - **Status**
   - Charter, governance board, release policy, communication plan, RFC/ADR templates, contribution/onboarding guides delivered (ADR-0001).
   - Issue/PR templates updated with DoR/DoD references.
   - Next: Populate governance board delegates, create recurring reports, and socialise process with consumer teams.

## Phase 2 – Tokens & Theming
- **Objectives**
  - Automate validation of `design-tokens.json` (contrast, scale ordering, brand coverage).
  - Provide tooling for brand previews, migrations, and fallback policies.
  - Harden regression protection through snapshot/approval tests and documentation for external extension.
- **Exit Criteria**
  - CI job failing on invalid tokens with actionable reports.
  - Brand management CLI or scripts documented and demoed.
  - Snapshot tests covering semantic/component tokens; docs explaining extension steps.
- **Workstreams**
  - Build validation tooling (C#/scripts) integrating `TokenValidator`.
  - Create brand preview experience (CLI + generated HTML/Storybook pages).
  - Document token update workflow, including review checklist and migration guidance.
  - Add analyzer or lint rule for unsafe hardcoded styling in components.

## Phase 3 – Components, UX & Accessibility
- **Objectives**
  - Ensure all components meet advanced requirements (states, density, high-contrast, error handling).
  - Reach WCAG 2.2 AA compliance, including keyboard/touch/ARIA coverage.
  - Backfill missing high-impact controls (complex tables, tree grid enhancements, notification system).
- **Exit Criteria**
  - Accessibility audit report closed out; automated axe/pa11y checks integrated.
  - Component maturity matrix updated to “Ready” or tracked in backlog with owners.
  - New/updated components shipped with documentation, story examples, and automated tests.
- **Workstreams**
  - Implement accessibility fixes (focus indicators, aria-live regions, reduced motion support).
  - Expand component variants (loading, compact, contextual help, validation messaging).
  - Develop high-priority components (hierarchical data tables, wizard, toast/notification center).
  - Profile performance hot-spots and introduce virtualization/lazy strategies where needed.

## Phase 4 – Developer Experience & Distribution
- **Objectives**
  - Deliver interactive documentation (Storybook or equivalent) with live playgrounds and code samples.
  - Provide scaffolding tools and analyzers to enforce best practices.
  - Automate packaging, signing, smoke tests, and release notes.
- **Exit Criteria**
  - Docs portal published with component catalogue, usage guides, and token explorer.
  - CLI/templates released for component scaffolding and theme extension.
  - CI/CD pipeline producing signed NuGet packages and validated against reference app.
- **Workstreams**
  - Stand up documentation stack (Storybook/DocFX) integrated into build.
  - Engineer analyzers for token misuse and component misuse scenarios.
  - Create reference application (Blazor Server/WASM) showcasing advanced scenarios.
  - Add visual regression tests (Playwright + snapshots) for key flows.

## Phase 5 – Support & Continuous Evolution
- **Objectives**
  - Establish telemetry, feedback, and support processes for consumers.
  - Implement regular planning (quarterly roadmap reviews, design system councils).
  - Ensure hotfix and long-term support flows are defined.
- **Exit Criteria**
  - Support playbook covering triage, SLA, escalation, and communication.
  - Feedback channels live (issue labels, dedicated syncs, survey cadence).
  - Quarterly roadmap template and review ritual documented.
- **Workstreams**
  - Instrument reference apps for usage metrics and error tracking (opt-in).
  - Launch feedback intake (form, office hours, community channel).
  - Define hotfix branching strategy and LTS policy; document consumer upgrade guidance.
- Schedule ongoing design system reviews with key product teams.

---

## Near-Term Backlog (Phase 2/3 precursors)
- **Token automation**: Add CI job invoking `TokenValidator` with contrast/scale checks, publish reports, and gate merges (tracked in `docs/TokenValidation.md`). ✅ (job running in CI).
- **Brand preview tooling**: Build CLI + generated Storybook pages for brand/theme visualization and approval workflows (`docs/BrandPreviewPlan.md`, `docs/BrandPreviewGuide.md`). _CLI + static gallery prototype ship as CI artifacts; PR summaries/comments now point straight to the gallery and the preview includes dialog interaction mock-ups. Next step is richer visualization/diffing._
- **Accessibility automation**: Integrate Playwright + axe suite covering priority components/states; store baselines for regression (`docs/AccessibilityAutomationPlan.md`). _Buttons, inputs, select combobox, tabs, and a sample dialog covered; next step is richer dialog interactions and visual baselines._
- **JS minimization**: Investigate reducing or eliminating JavaScript dependencies for rich controls (starting with `HaloSelect`), document trade-offs, and plan a JS-lite variant.
- **Documentation platform**: Stand up Storybook/doc site with live playgrounds and token explorer (aligned with Phase 4).
- **Telemetry hooks**: Instrument reference app for adoption metrics feeding `docs/MetricsPlan.md`.

## Cross-Cutting Backlog Seeds
- Performance budget definition and automated regression tracking.
- Security review of hosting model (dependency updates, supply chain, telemetry opt-ins).
- Integration guidelines for host apps (SSR, WASM, offline support) with sample code.
- Knowledge sharing plan (internal workshops, recorded walkthroughs, onboarding kits).

## Next Actions
- Assign phase leads and confirm staffing for Phase 0/1 activities.
- Create project board (e.g., GitHub Projects) seeded from Phase 0 inventory.
- Schedule kickoff aligning stakeholders on timeline, reporting cadence, and success metrics.
- ✅ Warning button variant validated across Light/DarkGlass; ghost button variant now first-class in Halo.
