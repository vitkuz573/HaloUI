# HaloUI Baseline Inventory

This document is the starting point for Phase 0 (Baseline Readiness). It captures the current asset catalogue and highlights immediate follow-ups for risk assessment. Status columns are intentionally left as `TBD` so workstream owners can update them during the audit.

---

## Component Catalogue

| Component | Location | Category | Maturity | Notes |
|-----------|----------|----------|----------|-------|
| `AriaInspector` | `Components/AriaInspector.razor` | Diagnostics | TBD | |
| `DialogBody` | `Components/DialogBody.razor` | Dialog | TBD | |
| `DialogFooter` | `Components/DialogFooter.razor` | Dialog | TBD | |
| `DialogHeader` | `Components/DialogHeader.razor` | Dialog | TBD | |
| `DialogHost` | `Components/DialogHost.razor` | Dialog | TBD | |
| `DialogInspector` | `Components/DialogInspector.razor` | Diagnostics | TBD | |
| `SnackbarHost` | `Components/SnackbarHost.razor` | Feedback | TBD | |
| `HaloBadge` | `Components/HaloBadge.razor` | Data display | TBD | |
| `HaloButton` | `Components/HaloButton.razor` | Actions | In Review | Warning variant tokens/styles validated across Light/DarkGlass; ghost variant now exposed via `ButtonVariant`. |
| `HaloCard` | `Components/HaloCard.razor` | Layout | TBD | |
| `HaloContainer` | `Components/HaloContainer.razor` | Layout | TBD | |
| `HaloDialog` | `Components/HaloDialog.razor` | Dialog | TBD | |
| `HaloExpandablePanel` | `Components/HaloExpandablePanel.razor` | Disclosure | TBD | |
| `HaloLabel` | `Components/HaloLabel.razor` | Typography | TBD | |
| `HaloLayout` | `Components/HaloLayout.razor` | Layout | TBD | |
| `HaloNotice` | `Components/HaloNotice.razor` | Feedback | TBD | |
| `HaloPasswordField` | `Components/HaloPasswordField.razor` | Forms | TBD | |
| `HaloRadioButton` | `Components/HaloRadioButton.razor` | Forms | TBD | |
| `HaloRadioGroup` | `Components/HaloRadioGroup.razor` | Forms | TBD | |
| `HaloSelect` | `Components/HaloSelect.razor` | Forms | TBD | |
| `HaloSelectOption` | `Components/HaloSelectOption.razor` | Forms | TBD | |
| `HaloSkeleton` | `Components/HaloSkeleton.razor` | Feedback | TBD | |
| `HaloSlider` | `Components/HaloSlider.razor` | Forms | TBD | |
| `HaloTab` | `Components/HaloTab.razor` | Navigation | TBD | |
| `HaloTable` | `Components/HaloTable.razor` | Data display | TBD | |
| `HaloTableColumn` | `Components/HaloTableColumn.razor` | Data display | TBD | |
| `HaloTabs` | `Components/HaloTabs.razor` | Navigation | TBD | |
| `HaloText` | `Components/HaloText.razor` | Typography | TBD | |
| `HaloTextArea` | `Components/HaloTextArea.razor` | Forms | TBD | |
| `HaloTextField` | `Components/HaloTextField.razor` | Forms | TBD | |
| `HaloToggle` | `Components/HaloToggle.razor` | Forms | TBD | |
| `HaloTreeView` | `Components/HaloTreeView.razor` | Navigation | TBD | |
| `HaloTreeViewNode` | `Components/HaloTreeViewNode.razor` | Navigation | TBD | |

**Next steps**
- Assign owners for maturity scoring and functional review.
- Confirm coverage of states (loading, error, success, disabled) and themes for each component.
- Capture gaps (missing components or states) in the shared backlog.

---

## Services & Diagnostics

| Service | Location | Purpose | Maturity | Notes |
|---------|----------|---------|----------|-------|
| `AriaDiagnosticsHub` | `Services/AriaDiagnosticsHub.cs` | Real-time accessibility diagnostics | TBD | |
| `AriaInspectorState` | `Services/AriaInspectorState.cs` | Aria inspector state management | TBD | |
| `DialogDiagnosticsHub` | `Services/DialogDiagnosticsHub.cs` | Dialog telemetry | TBD | |
| `DialogInspectorState` | `Services/DialogInspectorState.cs` | Dialog inspector state | TBD | |
| `DialogService` | `Services/DialogService.cs` | Dialog orchestration | TBD | |
| `NoOpAriaDiagnosticsHub` | `Services/NoOpAriaDiagnosticsHub.cs` | Diagnostics fallback | TBD | |
| `SnackbarService` | `Services/SnackbarService.cs` | Snackbar orchestration | TBD | |
| `ThemeState` | `Services/ThemeState.cs` | Theme state management | TBD | |

**Next steps**
- Validate DI registration paths and confirm public surface area.
- Document lifecycle expectations (thread safety, disposal, logging).
- Determine monitoring needs for diagnostics hubs in production scenarios.

---

## Token System Inventory

| Area | Assets | Coverage Notes | Owner | Follow-up |
|------|--------|----------------|-------|-----------|
| Manifest | `Theme/Tokens/design-tokens.json` | Contains current design tokens; review required for completeness | TBD | Validate merge process and versioning |
| Core Tokens | Color, Spacing, Border, Shadow, Typography, Transition | `Core/*.cs` | TBD | Check scale ordering, token naming consistency |
| Semantic Tokens | Color, Typography, Spacing | `Semantic/*.cs` | TBD | Map to component usage and accessibility constraints |
| Component Tokens | Badge, Button, Card, Container, Dialog, ExpandablePanel, Input, Label, Radio, Select, Skeleton, Slider, Snackbar, Tab, Table, Text, Toggle, TreeView | `Component/*.cs` | In Review | Warning button tokens added; confirm ghost tokens usage or deprecate. |
| Brand Tokens | `Brand/BrandTokens.cs` | TBD | Ensure brand palette generator covers all supported brands |
| Motion Tokens | `Motion/MotionTokens.cs` | TBD | Validate duration/easing coverage for interaction patterns |
| Responsive Tokens | `Responsive/ResponsiveTokens.cs` | TBD | Assess breakpoint strategy for large/small form factors |
| Accessibility Tokens | `Accessibility/AccessibilityTokens.cs` | TBD | Confirm focus, contrast, and touch target defaults |
| Variants | `Variants/ThemeVariants.cs` | TBD | Check density presets and host integration points |
| Generation Pipeline | Files under `Generation/` | TBD | Review generator accuracy, caching, and error reporting |
| Validation | `Validation/TokenValidator.cs` | TBD | Extend to CI enforcement and reporting |

**Next steps**
- Implement automated validation job covering contrast, scale monotonicity, and brand coverage.
- Establish snapshot/approval tests for semantic and component tokens.
- Document workflow for updating tokens, including reviewer checklist.

---

## Testing & Tooling Capture

| Area | Location | Current State | Follow-up |
|------|----------|---------------|-----------|
| Unit tests | `HaloUI.Tests/*` | TBD | Measure coverage, identify missing components/states. |
| Visual/interaction tests | N/A | TBD | Plan for Playwright + visual regression coverage. |
| Accessibility automation | N/A | TBD | Integrate axe/pa11y into test pipeline. |
| Build & analyzers | `.csproj`, analyzers | TBD | Confirm enforcement of styling/token usage. |
| Docs tooling | README, yet-to-be Storybook | TBD | Determine documentation platform and hosting. |

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
