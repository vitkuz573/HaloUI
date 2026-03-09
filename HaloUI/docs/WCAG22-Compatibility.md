# Halo WCAG 2.2 Compatibility Contract

This document defines the minimum accessibility contract enforced for `HaloUI` components.

## Normative Baseline

- WCAG 2.2 Recommendation: https://www.w3.org/TR/WCAG22/
- WAI-ARIA Authoring Practices Guide (APG): https://www.w3.org/WAI/ARIA/apg/
- APG patterns used directly for keyboard models:
  - Tabs: https://www.w3.org/WAI/ARIA/apg/patterns/tabs/
  - Tree View: https://www.w3.org/WAI/ARIA/apg/patterns/treeview/
  - Radio Group: https://www.w3.org/WAI/ARIA/apg/patterns/radio/
  - Combobox: https://www.w3.org/WAI/ARIA/apg/patterns/combobox/

## Enforced Rules (Component Layer)

- Every interactive component must expose an accessible name (visible label, `aria-label`, or `aria-labelledby`).
- Role-based controls that use ARIA contracts run strict runtime role validation via `AccessibilityAttributesBuilder.RequireCompliance()`.
- Selection controls and filter controls in `HaloTable` provide explicit contextual `aria-label` text.
- Live region semantics are opt-in for informational components (`HaloBadge`, `HaloNotice`) to avoid noisy screen-reader output.

## Quality Gates

- `HaloUI.Tests/AccessibilityCoverageContractTests.cs`:
  - Fails when a new public Halo component is added without explicit accessibility classification.
  - Fails when interactive components lack declared accessibility evidence files.
  - Fails when interactive components point to generic evidence that is not component-specific.
  - Fails when declared evidence files do not contain executable tests.
- Component-level accessibility tests exist in `HaloUI.Tests/`.
- Browser-level axe smoke checks run from `tests/accessibility/tests/*.spec.ts`.

## Scope

This contract guarantees WCAG compatibility at the Halo component layer.
Product-level WCAG conformance also depends on consumer content, page structure, copy, and workflow context.
