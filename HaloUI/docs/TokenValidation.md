# Token Validation Pipeline

HaloUI ships automated checks to ensure `design-tokens.json` remains WCAG-compliant and structurally sound.

## 1. Local Workflow
- Run `dotnet test HaloUI.Tests/HaloUI.Tests.csproj --filter FullyQualifiedName~TokenValidationTests` before opening a PR.
- Tests cover button/select/tab/token palettes, contrast validation, and disallow unsupported color keywords.
- Investigate failures by inspecting console output or `.trx` logs under `HaloUI.Tests/TestResults/`.

## 2. CI Enforcement
- `.github/workflows/ci.yml` includes a dedicated **Token Validation** job that runs on every push/PR.
- The job executes the same filtered test suite and uploads `token_validation.trx` as an artifact for review.
- CI is now blocked when validation errors occur; ensure tokens are updated or tests adjusted accordingly.
- Failure triage:
  1. Download the `token-validation-report` artifact.
  2. Inspect `token_validation.trx` via VS Test Explorer or `dotnet test --logger trx`.
  3. Compare failing token names against recent manifest changes; adjust values or update tests if expectations changed.
  4. If issues stem from design decisions, coordinate with the governance board and file a risk in `docs/Phase0-Findings.md`.

## 3. Reporting & Follow-up
- Governance board reviews recurring token issues in monthly KPI reports.
- Persistent violations should be captured as risks in `docs/Phase0-Findings.md` and mitigated with design sign-off.

---

_Extend this document when additional validation tooling (snapshot reports, contrast exports) is introduced._
