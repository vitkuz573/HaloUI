# Component Review Checklist

Use this checklist during refinement, implementation, and review of new or updated components. Attach a completed copy to the associated issue or PR.

---

## Design & Requirements
- [ ] Problem statement and user scenarios documented.
- [ ] Design assets (visuals, interaction flows) approved by design lead.
- [ ] States covered: default, hover, active, disabled, loading, error, success, empty.
- [ ] Density variants mapped (Comfortable, Compact, Touch where applicable).
- [ ] High-contrast and brand variants confirmed.

## Token & Theming Impact
- [ ] Semantic token usage reviewed; falls back to core tokens only when necessary.
- [ ] Component tokens defined/updated with naming aligned to conventions.
- [ ] Token generator impact assessed (regeneration executed successfully).
- [ ] CSS variable exposure verified via ThemeProvider.

## Accessibility
- [ ] Keyboard navigation paths defined and tested.
- [ ] Focus management and order explicit (including trap/release logic).
- [ ] ARIA roles, properties, and live region usage validated.
- [ ] Screen reader labelling/announcement behaviour verified.
- [ ] Reduced motion and touch target requirements satisfied.
- [ ] `HaloUI.Tests/AccessibilityCoverageContractTests.cs` updated for any new public component.
- [ ] Interactive components include explicit accessibility evidence files (bUnit/Playwright) and those files contain executable tests.

## Responsive Behaviour
- [ ] Component adapts to narrow containers and viewport breakpoints without layout breakage.
- [ ] Touch ergonomics validated (44px minimum target where interaction is expected).
- [ ] `HaloUI.Tests/ResponsiveCoverageContractTests.cs` updated for any new public component.
- [ ] Adaptive component stylesheet includes explicit responsive hooks (`@media`, `@container`, or fluid `clamp(...)` rules).

## Development
- [ ] Code follows STYLEGUIDE conventions (formatting, nullable, usings).
- [ ] Reusable logic extracted into base classes/services as needed.
- [ ] Error handling/logging strategy defined for diagnostics hubs.
- [ ] Analyzer/lint warnings addressed or waived with justification.

## Testing
- [ ] Unit/component tests added or updated for new behaviours.
- [ ] Interaction tests (Playwright/bUnit) cover critical flows.
- [ ] Visual regression snapshots (if applicable) updated.
- [ ] Accessibility automation (axe/pa11y) executed with results attached.
- [ ] Performance measurements captured for large data sets or complex scenarios.

## Documentation & Release
- [ ] Storybook/doc examples updated with usage snippets and guidance.
- [ ] Reference application (if impacted) updated.
- [ ] Release notes entry drafted, including migration tips if behaviour changed.
- [ ] Telemetry hooks documented or updated.
- [ ] Issue/PR links to DoR and DoD confirmations.

Add additional rows for component-specific needs (e.g., virtualization, internationalization).
