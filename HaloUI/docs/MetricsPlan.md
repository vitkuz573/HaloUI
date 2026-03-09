# HaloUI Metrics & Reporting Plan

Defines how we measure the health of the Halo product and report progress to stakeholders.

## 1. Key Performance Indicators (KPIs)
- **Adoption**
  - Number of active consumer apps per release.
  - Percentage of new features built with Halo.
- **Accessibility**
  - WCAG audit pass rate per release.
  - Count of Sev1/Sev2 accessibility defects escaping to production.
- **Reliability**
  - CI pass rate on first attempt.
  - Hotfix frequency per quarter.
- **DX Efficiency**
  - Lead time from issue “Ready” to merge.
  - Time to integrate a new component (surveyed from consumer teams).
- **Token Hygiene**
  - Token validation failures per release pipeline.
  - Time to resolve token regression alarms.

## 2. Data Sources
- AppVeyor builds (build/test results, duration).
- Test coverage reports (`dotnet test`, Playwright).
- Accessibility automation logs (axe/pa11y) + manual audit notes.
- Telemetry from reference apps (usage/error dashboards).
- Governance board surveys / office hour feedback.

## 3. Dashboards & Reporting
- **Monthly status update** shared with stakeholders summarising KPIs, risks, and notable wins (see `docs/reports/MonthlyStatus-template.md`).
- **Quarterly review** aligned with roadmap checkpoints; includes adoption deep dive and upcoming priorities.
- **Live dashboards** hosted via Azure Monitor / Power BI (TBD) aggregating CI health, accessibility, and adoption metrics.

## 4. Ownership
- Product Owner: overall KPI stewardship, monthly report author.
- Tech Lead: CI/build metrics, defect trends, token validation alarms.
- Accessibility Lead: WCAG audits, tooling runbooks, exception tracking.
- DevOps Lead: telemetry ingestion, dashboard maintenance.

## 5. Action & Escalation
- Threshold breaches (e.g., CI pass rate <90%, hotfixes >2 per quarter) trigger RCA and ADR if process changes are required.
- Risks recorded in `docs/Phase0-Findings.md` (Risk Register section) with mitigation owners.
- Metrics trends feed into roadmap reprioritisation during quarterly planning.

---

_Update this plan when KPIs or reporting cadences change; link updates via ADRs if the measurement strategy shifts substantially._
