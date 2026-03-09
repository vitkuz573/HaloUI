# Halo Responsive Compatibility Contract

This document defines the minimum responsive/adaptive contract for `HaloUI` components.

## Baseline

- Mobile-first behaviour with progressive enhancement.
- Adaptive behavior by both viewport and narrow container contexts.
- Touch ergonomics aligned with minimum 44x44 targets where applicable.
- Reduced-motion support (`prefers-reduced-motion`) for interactive/animated components.

## Enforced Rules

- Every public Halo component must be explicitly classified in `HaloUI.Tests/ResponsiveCoverageContractTests.cs`.
- Adaptive components with dedicated `*.razor.css` files must contain explicit responsive rules (`@media`, `@container`, or fluid `clamp(...)` usage).
- `ThemeProvider` must emit a global responsive foundation stylesheet (`Theme/ResponsiveFoundationCssBuilder.cs`) with:
  - touch optimization rules,
  - reduced-motion overrides,
  - baseline overflow/sizing guards.

## Scope

This contract guarantees a responsive baseline at the Halo component layer.
Application-level layout/content decisions in consuming apps can still affect final behavior.
