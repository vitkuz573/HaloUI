# Brand Preview Guide

Brand preview snapshots help reviewers inspect `design-tokens.json` changes without running the full application. The CLI prototype (`tools/BrandPreviewCli`) exports per-theme JSON + Markdown summaries, and CI publishes the artifacts automatically.

## Running Locally

```bash
dotnet run --project tools/BrandPreviewCli/BrandPreviewCli.csproj \
  --manifest HaloUI/Theme/Tokens/design-tokens.json \
  --themes Light,DarkGlass \
  --brands HaloUI,StartupHub \
  --output artifacts/brand-previews
```

Outputs:
- `artifacts/brand-previews/brands.json` – palette subset for selected brands.
- `artifacts/brand-previews/<theme>/<theme>.tokens.json` – semantic/component/motion/accessibility data.
- `artifacts/brand-previews/<theme>/<theme>.summary.md` – Markdown tables for quick review.
- `artifacts/brand-previews/themes.json` – aggregated theme data consumed by the gallery prototype.
- `artifacts/brand-previews/gallery/index.html` – static viewer that now includes dialog interaction mock-ups in addition to semantic/button/select/tab snapshots.

Use these files during code review to check color values, button states, and select/tab tokens.

## CI Integration

`.github/workflows/ci.yml` includes a `brand-preview` job that:
1. Runs the CLI with Light/DarkGlass + default brands.
2. Copies the gallery assets into the artifact and drops the aggregated `themes.json` next to them.
3. Uploads `brand-previews` artifacts to the workflow.
4. Adds a run summary + (for PRs) an auto-updated comment that links reviewers directly to the workflow run where the gallery artifact can be downloaded.

Reviewers can download the artifact from the GitHub Actions run and either open the Markdown summaries or serve the gallery (`tools/BrandPreviewCli/gallery/`) locally to browse the data visually.

## Gallery Prototype
- Folder: `tools/BrandPreviewCli/gallery/`
- Usage:
  ```bash
  cp artifacts/brand-previews/themes.json tools/BrandPreviewCli/gallery/
  cd tools/BrandPreviewCli/gallery
  python3 -m http.server 4173
  ```
  Then open `http://localhost:4173` to inspect tokens interactively.

### Dialog Interaction Preview
- The gallery now renders a miniature dialog using `component.Dialog` tokens (overlay, background, header/footer paddings) alongside `Button.Primary/Secondary/Ghost`.
- Use it to quickly verify overlay contrast, focus colors, and cross-variant pairing when dialog-related token changes are proposed.
- The close glyph adopts the Ghost palette to highlight high-contrast icon buttons, while the footer buttons pull semantic styles from the exported button tokens.

## Next Steps
- Wire the CLI output into Storybook or a static gallery for visual diffing.
- Add automated diffing between base/main artifacts to highlight token changes.
- Expand Markdown templates with contrast ratios and token validation summaries.

See `docs/BrandPreviewPlan.md` and RFC-0001 for the broader roadmap.
