# Definition of Ready & Definition of Done

These checklists align Phase 0 findings with future delivery work. Apply them to every component, token, or service change before scheduling backlog items. Update as governance evolves.

---

## Definition of Ready (DoR)

A work item is **Ready** when all of the following are true:

1. **Problem statement**: Clear description of the user or consumer need, including success criteria.
2. **Design references**: Approved mocks or interaction specs covering primary states (default, hover, active, disabled, loading, error, success) and high-contrast expectations.
3. **Token impact**: Token changes identified (new, modified, deprecated) with preliminary validation plan.
4. **Accessibility notes**: Expected ARIA roles/attributes, keyboard/touch interactions, focus handling, and reduced-motion requirements documented.
5. **Performance considerations**: Constraints or budgets identified (rendering cost, virtualization, payload size).
6. **Dependencies**: External services, packages, or cross-team input captured with owners.
7. **Testing strategy**: Agreed verification approach (unit, interaction, visual, accessibility automation).
8. **Acceptance criteria**: Testable statements defining done behaviour.
9. **Stakeholder sign-off**: Product/design/accessibility leads acknowledge readiness.

Use the DoR as a gating item during refinement; backlog issues without these details remain in “Needs Definition”.

---

## Definition of Done (DoD)

A work item is **Done** when:

1. **Implementation complete**: Code merged with peer review; analyzer/lint passes; no TODOs left unresolved.
2. **Token alignment**: Token changes validated via automated checks, snapshots updated, and documentation refreshed.
3. **Accessibility verified**: Manual keyboard/screen reader pass complete; automated tooling (axe/pa11y) executed with results linked.
4. **Testing**: Unit/component tests updated or added; integration/visual tests executed; coverage impact reviewed.
5. **Performance checks**: Profiling or benchmarks updated when applicable; performance budgets confirmed.
6. **Documentation**: Storybook/docs demos updated; README snippets or reference apps reflect new behaviour.
7. **Breaking change review**: Versioning implications evaluated; migration notes prepared if behaviour changed.
8. **Release notes**: Entry drafted in release log template (including token changes, accessibility notes).
9. **Stakeholder acknowledgment**: Product/design/accessibility leads confirm acceptance.
10. **Metrics hooks**: Telemetry and monitoring updated/validated if the feature feeds usage analytics.

Items that fail any DoD criteria remain in “In Review” or “Blocked” until resolved.

---

## Usage Tips

- Pair this document with `docs/Phase0-Findings.md` to prioritise remediation work.
- Link the DoR/DoD checklists in issue and PR templates so contributors attest to completion.
- Review quarterly to keep criteria aligned with evolving governance.
