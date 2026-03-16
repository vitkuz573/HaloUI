# HaloUI Baseline Inventory

This document captures the current HaloUI baseline and immediate follow-ups. Status columns are kept current to reduce drift between governance docs and implementation state.

---

## Component Catalogue

| Component | Location | Category | Maturity | Notes |
|-----------|----------|----------|----------|-------|
| `AriaInspector` | `Components/AriaInspector.razor` | Diagnostics | Operational | |
| `DialogBody` | `Components/DialogBody.razor` | Dialog | Operational | |
| `DialogFooter` | `Components/DialogFooter.razor` | Dialog | Operational | |
| `DialogHeader` | `Components/DialogHeader.razor` | Dialog | Operational | |
| `DialogHost` | `Components/DialogHost.razor` | Dialog | Operational | |
| `DialogInspector` | `Components/DialogInspector.razor` | Diagnostics | Operational | |
| `SnackbarHost` | `Components/SnackbarHost.razor` | Feedback | Operational | |
| `HaloBadge` | `Components/HaloBadge.razor` | Data display | Operational | |
| `HaloButton` | `Components/HaloButton.razor` | Actions | In Review | Warning variant tokens/styles validated across Light/DarkGlass; ghost variant now exposed via `ButtonVariant`. |
| `HaloCard` | `Components/HaloCard.razor` | Layout | Operational | |
| `HaloContainer` | `Components/HaloContainer.razor` | Layout | Operational | |
| `HaloDialog` | `Components/HaloDialog.razor` | Dialog | Operational | |
| `HaloExpandablePanel` | `Components/HaloExpandablePanel.razor` | Disclosure | Operational | |
| `HaloLabel` | `Components/HaloLabel.razor` | Typography | Operational | |
| `HaloLayout` | `Components/HaloLayout.razor` | Layout | Operational | |
| `HaloNotice` | `Components/HaloNotice.razor` | Feedback | Operational | |
| `HaloOtpField` | `Components/HaloOtpField.razor` | Forms | Operational | |
| `HaloPasswordField` | `Components/HaloPasswordField.razor` | Forms | Operational | |
| `HaloRadioButton` | `Components/HaloRadioButton.razor` | Forms | Operational | |
| `HaloRadioGroup` | `Components/HaloRadioGroup.razor` | Forms | Operational | |
| `HaloSelect` | `Components/HaloSelect.razor` | Forms | Operational | |
| `HaloSelectOption` | `Components/HaloSelectOption.razor` | Forms | Operational | |
| `HaloSkeleton` | `Components/HaloSkeleton.razor` | Feedback | Operational | |
| `HaloSlider` | `Components/HaloSlider.razor` | Forms | Operational | |
| `HaloTab` | `Components/HaloTab.razor` | Navigation | Operational | |
| `HaloTable` | `Components/HaloTable.razor` | Data display | Operational | |
| `HaloTableColumn` | `Components/HaloTableColumn.razor` | Data display | Operational | |
| `HaloTabs` | `Components/HaloTabs.razor` | Navigation | Operational | |
| `HaloText` | `Components/HaloText.razor` | Typography | Operational | |
| `HaloTextArea` | `Components/HaloTextArea.razor` | Forms | Operational | |
| `HaloTextField` | `Components/HaloTextField.razor` | Forms | Operational | |
| `HaloToggle` | `Components/HaloToggle.razor` | Forms | Operational | |
| `HaloTreeView` | `Components/HaloTreeView.razor` | Navigation | Operational | |
| `HaloTreeViewNode` | `Components/HaloTreeViewNode.razor` | Navigation | Operational | |

**Next steps**
- Assign owners for maturity scoring and functional review.
- Confirm coverage of states (loading, error, success, disabled) and themes for each component.
- Capture gaps (missing components or states) in the shared backlog.

---

## Services & Diagnostics

| Service | Location | Purpose | Maturity | Notes |
|---------|----------|---------|----------|-------|
| `AriaDiagnosticsHub` | `Services/AriaDiagnosticsHub.cs` | Real-time accessibility diagnostics | Operational | |
| `AriaInspectorState` | `Services/AriaInspectorState.cs` | Aria inspector state management | Operational | |
| `DialogDiagnosticsHub` | `Services/DialogDiagnosticsHub.cs` | Dialog telemetry | Operational | |
| `DialogInspectorState` | `Services/DialogInspectorState.cs` | Dialog inspector state | Operational | |
| `DialogService` | `Services/DialogService.cs` | Dialog orchestration | Operational | |
| `NoOpAriaDiagnosticsHub` | `Services/NoOpAriaDiagnosticsHub.cs` | Diagnostics fallback | Operational | |
| `SnackbarService` | `Services/SnackbarService.cs` | Snackbar orchestration | Operational | |
| `ThemeState` | `Services/ThemeState.cs` | Theme state management | Operational | |

**Next steps**
- Validate DI registration paths and confirm public surface area.
- Document lifecycle expectations (thread safety, disposal, logging).
- Determine monitoring needs for diagnostics hubs in production scenarios.

---

## Token System Inventory

| Area | Assets | Coverage Notes | Owner | Follow-up |
|------|--------|----------------|-------|-----------|
| Manifest | `Theme/Tokens/design-tokens.json` | Contains current design tokens; review required for completeness | HaloUI Core | Validate merge process and versioning |
| Core Tokens | Color, Spacing, Border, Shadow, Typography, Transition | `Core/*.cs` | HaloUI Core | Check scale ordering, token naming consistency |
| Semantic Tokens | Color, Typography, Spacing | `Semantic/*.cs` | HaloUI Core | Map to component usage and accessibility constraints |
| Component Tokens | Badge, Button, Card, Container, Dialog, ExpandablePanel, Input, Label, Radio, Select, Skeleton, Slider, Snackbar, Tab, Table, Text, Toggle, TreeView | `Component/*.cs` | In Review | Warning button tokens added; confirm ghost tokens usage or deprecate. |
| Brand Tokens | `Brand/BrandTokens.cs` | HaloUI Core | Ensure brand palette generator covers all supported brands |
| Motion Tokens | `Motion/MotionTokens.cs` | HaloUI Core | Validate duration/easing coverage for interaction patterns |
| Responsive Tokens | `Responsive/ResponsiveTokens.cs` | HaloUI Core | Assess breakpoint strategy for large/small form factors |
| Accessibility Tokens | `Accessibility/AccessibilityTokens.cs` | HaloUI Core | Confirm focus, contrast, and touch target defaults |
| Variants | `Variants/ThemeVariants.cs` | HaloUI Core | Check density presets and host integration points |
| Generation Pipeline | Files under `Generation/` | HaloUI Core | Review generator accuracy, caching, and error reporting |
| Validation | `Validation/TokenValidator.cs` | HaloUI Core | Extend to CI enforcement and reporting |

**Next steps**
- Implement automated validation job covering contrast, scale monotonicity, and brand coverage.
- Establish snapshot/approval tests for semantic and component tokens.
- Document workflow for updating tokens, including reviewer checklist.

---

## Testing & Tooling Capture

| Area | Location | Current State | Follow-up |
|------|----------|---------------|-----------|
| Unit tests | `HaloUI.Tests/*` | Operational | Measure coverage, identify missing components/states. |
| Visual/interaction tests | `HaloUI.Tests.E2E/*`, `tests/accessibility/tests/matrix-*.spec.ts` | Operational | Expand matrix state contracts as new sections/components land. |
| Accessibility automation | `tests/accessibility/tests/critical-a11y.spec.ts` | Operational | Keep WCAG-critical assertions aligned with component contracts. |
| Build & analyzers | `.csproj`, analyzers | Operational | Confirm enforcement of styling/token usage. |
| Docs tooling | README, yet-to-be Storybook | Operational | Determine documentation platform and hosting. |

---

## Phase 0 Checklist (Draft)

- [ ] Inventory complete with maturity scores and owners for each asset category.
- [ ] Risk register populated (accessibility, performance, DX, governance).
- [ ] Definition of Ready/Done agreed and documented.
- [ ] Backlog entries created for high-priority gaps and technical debt.
- [ ] Kickoff scheduled for Phase 1 with stakeholders and timelines.

---

## Open Questions

1. Which teams rely on each component today, and what SLAs do they require?
2. What environments must tokens accommodate (light/dark, high contrast, branding, offline)?
3. Are there regulatory/compliance constraints influencing accessibility or branding?
4. Which metrics (usage, error rate, bundle size) should be tracked for ongoing health?

Populate answers during Phase 0 interviews and system reviews.
