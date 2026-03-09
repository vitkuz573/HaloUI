# Architecture Decision Records (ADRs)

Use ADRs to capture significant technical/governance decisions for HaloUI. Each ADR documents the context, decision, and consequences so future contributors understand why the system evolved a certain way.

## Process
1. When a decision is proposed (often after an RFC), create a new ADR file in this folder using the template.
2. Name files sequentially: `0001-token-validation-ci.md`, `0002-storybook-stack.md`, etc.
3. Submit the ADR via pull request and reference related issues/RFCs.
4. Once approved, avoid editing historical ADRs—add follow-up ADRs instead.

## Folder Structure
```
docs/adr/
  README.md
  0000-template.md
  0001-sample-decision.md (example, optional)
```

## Template
Copy `0000-template.md` to start a new ADR. Keep the status updated (`Proposed`, `Accepted`, `Deprecated`, etc.) and include clear consequences for implementation and consumers.

---

_Questions? Reach the governance board via #haloui._
