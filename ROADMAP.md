# HaloUI Roadmap

This roadmap is live for the current major line. No parallel `v2` track is maintained.

## 2026 Q1: Runtime Hardening (In Progress)
- Convert theme descriptor icon metadata to strongly typed `HaloIconToken`.
- Standardize JS interop through module runtime abstractions (`Select`, `Overlay`, `Measurement`, `Theme DOM`).
- Enforce public API snapshot contracts in CI.
- Expand component and demo state contracts for matrix testing.

## 2026 Q2: Coverage and Performance Gates
- Add strict budget checks for HaloUI runtime JS and scoped CSS bundle.
- Expand Playwright matrix with state-contract assertions across viewports/themes.
- Increase component contract fidelity for disabled/readonly/error/loading states.
- Raise CI artifact quality (coverage manifests, accessibility reports, deterministic screenshots).

## 2026 Q3: Packaging and Consumption
- Finalize icon-pack distribution model for first-party packs.
- Strengthen integration smoke suites for host applications consuming HaloUI.
- Improve migration notes automation for breaking API updates.

## 2026 Q4: Theming and Brand Scale
- Deepen brand token workflows and snapshot tooling.
- Add governance automation for token drift and release readiness.
- Tighten performance budgets based on production telemetry.

## Exit Criteria
- `dotnet build HaloUI.slnx` and `dotnet test HaloUI.slnx` remain green.
- Playwright matrix + critical a11y + perf budget checks are mandatory and green in CI.
- Public API contract file is in sync with the shipped surface.
