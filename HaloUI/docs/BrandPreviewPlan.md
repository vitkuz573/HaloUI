# Brand Preview Tooling Plan

This document captures the design for multi-brand preview tooling referenced in Roadmap Phase 2.

## Goals
- Provide designers and engineers with instant previews of all brands/themes (Light, DarkGlass, future partners) without running the full app.
- Generate shareable artifacts (static HTML/Storybook stories) that highlight tokens, component states, and contrast metrics.
- Support CI snapshots to detect regressions when tokens change.

## Proposed Architecture
1. **Token Snapshot CLI**
   - Location: `tools/ThemeSdk.SnapshotTool` (extend existing generator) or new `tools/BrandPreview`.
   - Inputs: `design-tokens.json`, list of brands/themes, optional overrides.
   - Outputs: JSON + Markdown describing palette, contrast tables, and CSS variable dumps.

2. **Preview Gallery**
   - Build a lightweight static site (Vite/React or Razor pages) that consumes CLI output and renders:
     - Token tables grouped by semantic/component category.
     - Component mock-ups (buttons, tabs, select, snackbar, etc.) referencing generated CSS variables.
   - Deploy as part of Storybook or GitHub Pages artifact in CI.

3. **CI Integration**
   - Add workflow job to run CLI on every token change.
   - Publish artifacts (HTML + JSON) as build downloads; future phase can compare diffs for regressions.

## Deliverables
- [x] CLI spec & prototype (tools/BrandPreviewCli) emitting per-theme JSON + Markdown summaries.
- [x] Static gallery prototype with brand selector (`tools/BrandPreviewCli/gallery`).
- [x] Documentation (`docs/BrandPreviewGuide.md`) describing usage and review process.
- [x] CI job uploading previews for PR review (see `.github/workflows/ci.yml` job `brand-preview`).
- [x] PR-visible hooks (job summary + sticky PR comment) pointing reviewers directly to the gallery artifact, now showcasing dialog interactions.

## Dependencies & Risks
- Requires alignment with Design Systems for acceptable preview format.
- Snapshot size may be large; plan retention strategy.
- Need to ensure generated CSS stays in sync with main Theme SDK (shared generator recommended).

## Next Steps
1. Draft RFC covering CLI schema + preview UX (assign ID `RFC-0001` placeholder). ✅
2. Spike on extending `ThemeSdk.SnapshotTool` to emit per-brand bundles. (Handled via `tools/BrandPreviewCli` prototype.)
3. Evaluate Storybook vs. custom static site for hosting.
4. Update roadmap once spike confirms estimates.

## CLI Usage
```bash
dotnet run --project tools/BrandPreviewCli/BrandPreviewCli.csproj \
  --manifest HaloUI/Theme/Tokens/design-tokens.json \
  --themes Light,DarkGlass \
  --brands HaloUI,StartupHub \
  --output artifacts/brand-previews
```

Outputs:
- `brand-previews/brands.json` – subset of brand palettes.
- `brand-previews/<theme>/<theme>.tokens.json` – semantic/component/motion/accessibility tokens.
- `brand-previews/<theme>/<theme>.summary.md` – Markdown summary (colors, button/select/tab snapshots).

---

_Use this plan as input for the upcoming RFC; update sections as decisions land._
