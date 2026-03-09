# HaloUI Release Policy

Defines how Halo versions are produced, supported, and communicated. Applies to both the Blazor component library and associated token/tooling packages.

---

## 1. Versioning Strategy
- **Semantic Versioning (SemVer)**: `MAJOR.MINOR.PATCH`
  - **MAJOR** – Breaking API changes, token restructures, or behaviour requiring consumer action.
  - **MINOR** – Backward-compatible features, new components, token additions, or non-breaking accessibility/performance enhancements.
  - **PATCH** – Bug fixes, security updates, doc-only corrections (`MAJOR`/`MINOR` unchanged).
- **Pre-release tags**: `-alpha.N`, `-beta.N`, `-rc.N` for early adopters. Beta/RC builds must satisfy DoR/DoD minus GA sign-off.
- **Compatibility window**: Latest minor + previous minor fully supported; earlier versions receive security fixes only if mandated.

## 2. Cadence & Scheduling
- **Monthly planned releases** (first Tuesday) containing accumulated MINOR/PATCH changes.
- **Quarterly alignment**: Every third release includes roadmap checkpoint + documentation refresh.
- **Hotfixes**: Triggered by Sev1/Sev2 regressions (accessibility failures, build breaks, security issues). Target <48h turnaround from triage to publish.
- **Freeze window**: 48h before planned release to complete validation (CI pass, manual a11y run, documentation updates).

## 3. Readiness Gates
1. All automated pipelines green (build, unit tests, analyzers, token validation, visual/a11y tests when available).
2. Release checklist completed:
   - DoR/DoD confirmed for included work items.
   - Risk register updated for known issues.
   - Release notes drafted with sections: Highlights, Breaking Changes, Token Updates, Accessibility Verification, Known Issues.
3. Stakeholder sign-off (Product, Design, Accessibility, DevOps).
4. Packages built via reproducible pipeline with signing (NuGet + npm artifacts when applicable).

## 4. Packaging & Distribution
- **NuGet**: `HaloUI`, `HaloUI.Tests` helpers (if published), analyzers/generators.
- **npm**: Generated CSS/asset bundle for host applications (optional; tracked separately).
- **Artifacts** stored in Azure Artifacts (internal) with public mirror when approved.
- Each build produces SBOM and integrity manifest; artifacts stored for ≥1 year.

## 5. Communication Plan
- **Release Notes**: Markdown document in `docs/releases/` + summary in repo root CHANGELOG (if introduced). Include upgrade guidance and validation evidence.
- **Announcement Channels**: Internal Slack `#haloui`, email digest, and entry in platform status page.
- **Consumer Notification**: Teams subscribed via governance board receive direct ping for breaking changes and token migrations.
- **Documentation Updates**: Storybook/demo site redeployed alongside release.

## 6. Support & Maintenance
- **Active support**: Latest two minor versions; critical fixes ported when feasible.
- **Extended support**: Older versions receive security fixes only; communication sent one quarter before EOL.
- **Telemetry & Feedback**: Monitor crash/usage dashboards post-release; open feedback window (1 week) for adopter reports.
- **Deprecation Process**:
  1. Announce in release notes with migration plan.
  2. Provide analyzer warnings or feature flags.
  3. Remove in next MAJOR.

## 7. Roles & Responsibilities
- **Release Manager (rotating)**: Orchestrates checklist, freeze, approvals.
- **Build/DevOps**: Maintains pipelines, signing credentials, package publishing.
- **Component Owners**: Ensure documentation/tests updated for their deliverables.
- **Comms Lead**: Publishes release notes and announcements.
- **Support Lead**: Monitors telemetry, triages escalations.

## 8. Change Request Workflow
1. Work item passes DoR and enters sprint backlog.
2. Implementation occurs on feature branches; PRs linked to issues.
3. Once merged, entry added to curated release board.
4. Prior to release freeze, Release Manager curates content and verifies readiness gates.
5. Tags created (`vX.Y.Z`), packages published, documentation deployed.
6. Post-release review within 5 business days (retro, KPIs, incident review).

## 9. Templates & References
- Release checklist + notes template: `docs/releases/ReleaseNotes-template.md`
- Governance/RFC process: `docs/Phase1-Governance.md`, `docs/rfc/`
- Charter & responsibilities: `docs/Charter.md`

---

_Maintained by the Halo governance board. Update alongside major process changes._
