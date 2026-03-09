# HaloUI Onboarding Guide

Use this playbook to ramp up new engineers, designers, or QA working on the HaloUI.

## 1. First Week Checklist
- [ ] Read the product charter (`docs/Charter.md`) and roadmap (`ROADMAP.md`).
- [ ] Review Phase 0 findings and risk register (`docs/Phase0-Findings.md`).
- [ ] Walk through component/token architecture (`HaloUI/README.md`).
- [ ] Set up development environment: `dotnet build`, `dotnet test HaloUI.Tests`, `npm ci` in `HaloUI.Server` if working on UI assets.
- [ ] Meet the governance board representative for your discipline (see `docs/GovernanceBoard.md`).

## 2. Required Training
- **Accessibility bootcamp**: Attend the next Accessibility Lead session; review WCAG checklists and tooling runbooks.
- **Token pipeline walkthrough**: Pair with Design/Tech lead to understand `design-tokens.json`, generators, and validation.
- **Tooling orientation**: Learn analyzer warnings, lint rules, and CI expectations.
- **Documentation workflow**: Publish a sample update (README or docs) and follow PR template.

## 3. Buddy Tasks
Complete with an assigned buddy:
1. Fix a small documentation issue or add a missing test.
2. Shadow a PR review focused on a11y/tokens.
3. Run token validation + tests locally and attach results to an issue.

## 4. Access & Tools
- GitHub repo access with ability to run CI.
- Storybook/preview environment (when available).
- Slack `#haloui` and shared drive for recordings.
- Azure DevOps / telemetry dashboards (request via DevOps Lead).

## 5. Expectations
- Adhere to DoR/DoD, checklists, and contribution guide (`docs/ContributionGuide.md`).
- Document decisions via ADRs or update existing docs when behaviour changes.
- Participate in office hours or stand-ins when representing your discipline.

## 6. Escalation & Support
- Buddy or team lead for day-to-day questions.
- Governance board for prioritisation conflicts.
- Accessibility/Design leads for UX or compliance blockers.

---

_Review this guide quarterly; incorporate feedback from new joiners._
