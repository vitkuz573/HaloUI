# HaloUI Lifecycle Overview

Captures the end-to-end flow for components and tokens from proposal to release.

## 1. Component Lifecycle
```
Ideation
  ↓ (Definition of Ready: problem, design, a11y, tokens, tests)
Backlog Ready
  ↓ (RFC required?)
Design & Spec
  ↓ (Review: product, design, a11y)
Implementation
  ↓ (Unit/bUnit, Playwright, axe, docs)
Review & Validation
  ↓ (DoD checklist, release notes, ADR if needed)
Release & Support
```

Key checkpoints:
- DoR/DoD checklists enforced via issue/PR templates.
- Accessibility validation must happen before merge.
- Release notes drafted for each merge that impacts consumers.

## 2. Token Lifecycle
```
Design request → Manifest update (`design-tokens.json`)
      ↓
Validation (TokenValidator, contrast, scale ordering)
      ↓
Generator run + diff review
      ↓
Tests & visual validation (components consuming tokens)
      ↓
Docs & migration guidance
      ↓
Release (SemVer, communication plan)
```

Rules:
- Tokens are source of truth; components consume typed records only.
- Breaking token changes require RFC + migration plan.
- Snapshot/approval tests guard against regressions.

## 3. Governance Touchpoints
- RFC approval precedes major lifecycle steps.
- ADR recorded for architecture/process decisions.
- Governance board reviews lifecycle adherence during syncs.

## 4. References
- DoR/DoD: `docs/DefinitionOfReadyDone.md`
- Checklists: `docs/checklists/Component.md`, `docs/checklists/Token.md`
- Contribution guide: `docs/ContributionGuide.md`
- Release policy: `docs/ReleasePolicy.md`
- Communication plan: `docs/CommunicationPlan.md`

---

_Use this document in onboarding and roadmap reviews to remind teams of required gates._
