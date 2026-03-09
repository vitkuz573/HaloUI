# HaloUI RFCs

Formal proposals for non-trivial changes (new components, token restructures, governance updates). Use this process when work spans multiple teams or introduces potential breaking changes.

## Workflow
1. Open an issue describing the intent and link to draft RFC.
2. Create a copy of `template.md`, name it `XXXX-title.md` (where `XXXX` is incremental ID).
3. Submit PR with the RFC for review; label as `RFC`.
4. Governance board reviews within 5 business days.
5. Approved RFCs move to `docs/rfc/accepted/`; rejected/withdrawn go to `docs/rfc/archive/`.

## Folder Structure
```
docs/rfc/
  README.md
  template.md
  accepted/
  archive/
```

Create `accepted/` and `archive/` directories as needed. Keep historical RFCs immutable—use follow-up RFCs for changes.

## Review Expectations
- Include product, design, accessibility, and engineering reviewers.
- Capture open questions and risks.
- Link to ADRs or backlog items when implementation begins.

---

_Questions? Ping the governance board or #haloui channel._
