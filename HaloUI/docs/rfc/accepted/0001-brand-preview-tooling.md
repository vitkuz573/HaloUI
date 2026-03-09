# RFC-0001: Brand Preview Tooling & Token Snapshot Pipeline

- **Authors**: Codex Agent, @vitkuz573
- **Status**: Draft
- **Created**: 2024-05-29
- **Updated**: 2024-05-29
- **Tracking Issues**: #000 (placeholder)

## 1. Summary
Introduce a reproducible tooling stack that generates per-brand/theme previews (HTML + JSON) from `design-tokens.json`, enabling designers and engineers to verify token changes without running the full app. Artifacts will feed Storybook/CI workflows and support regression review.

## 2. Motivation
- Designers need quick comparisons of Light/DarkGlass/custom brands to validate palettes and contrast before merging changes.
- Current manual screenshots are error-prone and slow; there’s no audit trail of what changed between releases.
- Tooling aligns with Phase 2 roadmap exit criteria (brand management CLI + snapshots).

### Non-goals
- Building a full public doc site (covered in Phase 4).
- Replacing in-app visual tests; previews complement existing component tests.

## 3. Detailed Design
### Components
1. **Token Snapshot CLI**
   - Extend `tools/ThemeSdk.SnapshotTool` (or add `tools/BrandPreview`) to emit:
     - Per-brand JSON (semantic/component tokens, derived CSS variables).
     - Markdown/CSV tables summarizing contrast ratios and state coverage.
   - CLI parameters: `--brands Light DarkGlass --output out/brand-previews`.

2. **Preview Gallery**
   - Build a static site (Vite/React) or Storybook stories that ingest CLI output.
   - Render:
     - Palette cards grouped by semantic/component category.
     - Component mock-ups referencing CSS variables (buttons, tabs, select, snackbar, etc.).
     - Contrast callouts (pass/fail badges).

3. **CI Integration**
   - New CI `brand-preview` step:
     1. Run CLI for all supported brands.
     2. Build static gallery.
     3. Upload artifact for review (long term: publish to a static preview environment).

### Validation Strategy
- Reuse `TokenValidator` outputs to annotate previews (e.g., highlight borderline contrast).
- Compare generated JSON across runs for diff-based approval (future enhancement).

### Migration
- Consumers keep using existing components; preview artifacts are for reviewers.
- Document usage in `docs/BrandPreviewPlan.md` + README.

## 4. Alternatives Considered
- **Manual Storybook stories** per brand: rejected—high maintenance, duplicates data.
- **Screenshots from host app**: rejected—slow, environment-specific, lacking metadata.

## 5. Risks & Mitigations
- **Tooling drift**: Ensure CLI reuses Theme SDK generator to avoid mismatches.
- **Artifact size**: Limit image generation; compress assets; prune retention.
- **Adoption**: Provide runbook and embed links in PR template.

## 6. Dependencies
- Existing `ThemeSdk.SnapshotTool`.
- Node/Storybook stack (already used for server UI build).
- CI artifacts storage.

## 7. Rollout Plan
1. Spike CLI extension and output schema (Week 1).
2. Build minimal preview gallery with 2–3 component showrooms (Week 2).
3. Add CI job + artifact upload (Week 3).
4. Socialize usage via docs/office hours; iterate based on feedback.

## 8. Open Questions
- Should previews include motion tokens/animations?
- Where to host artifacts long-term (internal portal vs. static preview host)?
- Do we need snapshot diff tooling before GA?

## 9. Appendix
- `docs/BrandPreviewPlan.md`
- `docs/TokenValidation.md`
- `HaloUI.ThemeSdk.SnapshotTool`

---

_Upon approval, convert to Accepted and track implementation via linked issues._
