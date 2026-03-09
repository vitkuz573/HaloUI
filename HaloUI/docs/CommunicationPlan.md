# HaloUI Communication & Change Management Plan

Provides consistent messaging for releases, roadmap updates, and incident response across consumer teams.

## 1. Audiences
- **Core consumers** (Server Console, Host Agents, Admin Portal teams)
- **Stakeholders** (Product, Design, Accessibility, Platform Eng leadership)
- **Contributors** (internal developers/designers)
- **External partners** (integrators leveraging the HaloUI SDK)

## 2. Channels
- `#haloui` Slack channel (day-to-day updates, questions)
- Email distribution list for release announcements and incident summaries
- Documentation portal / Storybook for public-facing updates
- Quarterly review meetings (roadmap & KPI walkthrough)
- Office hours (bi-weekly) for deep dives and support

## 3. Cadences
- **Release Announcements**: day of release, referencing `docs/releases/<version>.md` and highlighting validation.
- **Roadmap Updates**: quarterly, aligned with Phase roadmap; published to README/ROADMAP and presented live.
- **Incident/Hotfix Notices**: within 1 hour of Sev1/Sev2 confirmation; follow-up postmortem within 5 business days.
- **Feedback Surveys**: twice per year to gauge DX satisfaction and adoption blockers.

## 4. Message Templates
- Release note summary (link to template file).
- Hotfix alert: severity, impact, workaround, ETA.
- Breaking change heads-up: description, migration guide, timelines, contact.

## 5. Responsibilities
- Product Owner: curates messaging, ensures roadmap alignment.
- Comms Lead (TBD): coordinates announcements, maintains mailing list.
- Tech Lead: provides technical details for releases/incidents.
- Accessibility Lead: shares audit outcomes and accessibility exceptions.
- Documentation Lead: updates Storybook/docs to match announcements.

## 6. Tooling & Tracking
- Maintain announcement log in `docs/releases/` alongside release notes.
- Track communication tasks on governance board project (labels: `comms`, `enablement`).
- Archive recordings/slides from roadmap reviews in shared drive (link TBD).

## 7. Enablement Schedule
- **Design system office hours**: Bi-weekly (Wednesdays 15:00 UTC) hosted by Product + Design leads; capture Q&A notes in shared doc.
- **Accessibility deep dives**: Monthly session led by Accessibility Lead reviewing audits and upcoming requirements.
- **Release clinic**: 1 week before scheduled release to walk through checklists, token diffs, and documentation updates.
- Owners: Product Owner schedules calendar invites; Documentation Lead maintains agendas and recordings.

## 8. Change Management Workflow
1. Identify change (feature, token update, breaking behaviour).
2. Assess impact and required communication tier (FYI, advisory, urgent).
3. Draft message referencing relevant docs/RFCs.
4. Secure approvals from Product + Tech + Accessibility leads.
5. Publish via channels above; monitor for follow-up questions.
6. Record in communication log and update risk register if needed.

---

_Review the plan quarterly; update channel owners or cadences as the program matures._
