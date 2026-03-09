# Token Update Checklist

Complete this checklist when modifying `design-tokens.json` or associated generated assets. Attach to issue or PR for visibility.

---

## Planning
- [ ] Change description includes rationale, affected components, and target release.
- [ ] Impact analysis identifies dependent brands, variants, and themes.
- [ ] Accessibility considerations captured (contrast, readability, motion).
- [ ] Performance considerations noted (bundle size, re-render costs).

## Manifest & Generation
- [ ] `design-tokens.json` updated with consistent naming and structure.
- [ ] Validation tooling executed (contrast, scale monotonicity, format checks).
- [ ] Generator run locally; regenerated files reviewed for expected diffs.
- [ ] Brand palette adjustments verified across default and custom brands.
- [ ] Variant mappings reviewed (Compact, Comfortable, Touch, etc.).

## Testing
- [ ] Snapshot/approval tests updated and passing.
- [ ] Unit tests for token consumers adjusted if required.
- [ ] Visual regression run for impacted components.
- [ ] Accessibility automation rerun for affected colour/spacing changes.

## Documentation & Communication
- [ ] Token change log updated with before/after context.
- [ ] Docs/Storybook refreshed to display new token values or guidance.
- [ ] Migration guidance prepared (including fallback recommendations).
- [ ] Release notes draft created with highlights and testing summary.
- [ ] Stakeholders (design, accessibility, implementation owners) sign off.

## Post-Merge
- [ ] CI validation monitored for downstream failures.
- [ ] Telemetry/feedback channels notified to watch for regressions.
- [ ] Backlog updated with follow-up tasks (if partial rollout).

Adjust checklist entries as additional validation tooling or brand requirements emerge.
