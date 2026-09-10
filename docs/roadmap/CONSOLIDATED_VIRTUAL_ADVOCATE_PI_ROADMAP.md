# Consolidated Virtual Advocate PI Roadmap

## App

Virtual Advocate PI

## Purpose

This roadmap combines the main Virtual Advocate PI development stream, the MAUI/mobile stream, and the GARP M 2026 changes stream into one project view.

## Current project position

Virtual Advocate PI now has a working web/backend foundation with Firebase authentication, Cloud Run API deployment, PostgreSQL/EF persistence, claim workspace workflows, evidence tooling, AI/RAG draft support, generated document workflows, admin knowledge management and a MAUI mobile MVP foundation.

## Major capabilities already built

### Web app and backend

- Firebase sign-in and authenticated backend access.
- Claim workspace navigation.
- Condition management.
- Accepted history recording.
- Guided questions.
- GARP M questions.
- GARP M summary.
- Evidence metadata.
- Evidence upload.
- Evidence gaps.
- Evidence checklist.
- Workspace audit trail.
- AI draft generation and review workflow.
- Generated documents page.
- Claim Starter Pack document generation.
- Doctor Guidance Pack document generation.
- Document download flow.
- Admin dashboard.
- Admin access control.
- Admin source metadata manager.
- Admin question and document template editor.
- Admin prompt and disclaimer versioning.
- Admin knowledge audit review.
- Admin audit logging.
- Admin smoke test checklist.

### Cloud/backend

- Cloud Run backend service.
- Firebase token validation through backend user service.
- PostgreSQL/EF Core persistence.
- EF migrations for source registry, template registry and prompt/disclaimer registry.
- Admin database maintenance endpoint.
- Approved source registry seeding.
- Mobile session endpoint for authenticated mobile clients.

### MAUI/mobile

- MAUI project shell created.
- Shared services folder structure created.
- Mobile environment settings added.
- API health check added.
- Firebase email/password sign-in integrated.
- Secure token storage added.
- Sign-out added.
- Authenticated API client added.
- Mobile /api/v1/mobile/me token flow confirmed.
- Windows MAUI build passed.
- Android MAUI build passed.

## Completed phase summary

### Phase 7 - AI/RAG knowledge base

- AI/RAG architecture and safety plan.
- Approved source registry and knowledge base structure.
- Source category taxonomy.
- GARP M, DVA reform references and internal templates loaded into the knowledge base foundation.
- Ingestion and metadata rules.
- Retrieval API.
- Prompt templates and safety guardrails.
- AI draft request and review workflow.
- AI/RAG audit logging.
- AI/RAG smoke test checklist.

### Phase 8 - Generated document integration and approved draft inclusion

- Claim Starter Pack document generation foundation.
- DOCX generation service.
- PDF/storage/versioning preparation.
- Download flow.
- Reviewed-only content guardrails.

### Phase 9 - Doctor Guidance Pack

- Doctor guidance template foundation.
- Clinical question generation workflow.
- Doctor-facing disclaimer and review checklist.
- Doctor Guidance Pack export workflow.

### Phase 10 - Admin knowledge and template manager

- Admin roles and access control.
- Admin dashboard shell and navigation.
- Source metadata manager.
- Question and document template editor.
- Prompt and disclaimer versioning.
- Knowledge base audit review view.
- Admin audit logging.
- Admin smoke test checklist.
- Phase 10 close-out completed.

### Phase 11 - Android and iOS app MVP

Code complete. See docs/phase-11/PHASE_11_CLOSEOUT_STATUS.md.

Done:

- MAUI app shell and shared services.
- Mobile API environment settings.
- Firebase Authentication.
- Authenticated API client and token flow.
- Dashboard, disclaimer gate and claim workspace screens.
- Condition intake and GARP M question engine / structured summary screens.
- Evidence screen: preparation checklist, per-condition evidence items,
  file pick + signed-URL upload + mark-uploaded, open uploaded files,
  evidence gaps with recalculate.
- AI draft review screen: read, edit your version, set review status,
  archive (no on-device generation).
- Generated documents screen: generate Claim Starter Pack / Doctor Guidance
  Pack, download DOCX/PDF via signed URL.
- Mobile loading, error and empty states (including a conditions-load
  error + retry fix on the GARP M screens).
- Android app icon, name and Release packaging config (aab, signing at
  build time).
- iOS app icon, name and Release config (written, unverified - needs a Mac).
- Phase 11 mobile smoke test checklist
  (docs/phase-11/PHASE_11_MOBILE_SMOKE_TEST_CHECKLIST.md).

Pending (tester, needs real hardware / a Mac):

- iOS build on macOS.
- Full authenticated end-to-end pass on Android and iOS physical devices.
- Milestone: Phase 11 complete.

### Phase 12 - Functional and Load Evidence tools

Not started. Authoritative task list is in ProjectLibre
(Veteran_Connect_Post_2026_ProjectLibre.xml.pod). The ProjectLibre phase
header still reads "Security, privacy and production hardening" - that is a
stale label; the phase content is the functional and load evidence tools
below. Production hardening is now Phase 14.

Tasks:

- Register uploaded functional/load evidence templates.
- Create functional impact question bank.
- Create Good Day / Bad Day builder.
- Create load bearing and load factor data model.
- Create lifting and carrying exposure builder.
- Create stairs, ladders and rungs exposure builder.
- Create kneeling and squatting exposure builder.
- Create neck and shoulder load carriage builder.
- Create heavy load carrying builder.
- Create RAN/equipment weight reference library.
- Create hazard exposure table builder.
- Add admin templates for functional/load evidence tools.
- Add prompt and disclaimer versions for functional/load evidence tools.
- Add Claim Starter Pack DOCX sections.
- Add Doctor Guidance Pack DOCX sections.
- Add PDF export support for evidence annexes.
- Add Functional and Load Evidence smoke test checklist.
- Milestone: Phase 12 complete.

## GARP M 2026 changes stream

### Purpose

The GARP M 2026 changes stream tracks the July 2026 GARP M update and ensures Virtual Advocate PI can separate current GARP M behaviour from post-July 2026 guidance, source references, questions, summaries and document wording.

### GARP M 2026 features to add to the roadmap

- Compare existing GARP M references with the July 2026 GARP M update.
- Store GARP M 2026 source documents in the approved source registry.
- Add source version tagging for current GARP M versus July 2026 GARP M.
- Add admin review workflow for GARP M source changes.
- Update GARP M question engine content where the July 2026 changes require new wording or factors.
- Update GARP M summary generation so it clearly identifies the applicable GARP M version.
- Update evidence mapping for changed GARP M factors.
- Update Claim Starter Pack templates for current versus July 2026 GARP M context.
- Update Doctor Guidance Pack templates for current versus July 2026 GARP M context.
- Add post-July 2026 disclaimer wording so the app does not imply legal advice, medical advice, impairment calculation, compensation estimate, DVA decision-making or guaranteed claim outcome.
- Add GARP M 2026 audit events for source, template and prompt changes.
- Add GARP M 2026 smoke test checklist.

### Phase 13 - GARP M 2026 change integration

Not started. Authoritative task list is in ProjectLibre.

Tasks:

- Import and register GARP M 2026 source documents.
- Complete current-versus-July-2026 GARP M comparison note.
- Add GARP M version metadata to source registry.
- Add GARP M version metadata to question templates.
- Add GARP M version metadata to document templates.
- Update GARP M question engine content.
- Update GARP M summary content.
- Update evidence checklist mappings.
- Update Claim Starter Pack template wording.
- Update Doctor Guidance Pack template wording.
- Add GARP M 2026 prompt versions.
- Add GARP M 2026 disclaimer versions.
- Add admin audit filters for GARP M 2026 events.
- Add GARP M 2026 smoke test checklist.
- Milestone: Phase 13 complete.

## Recommended next roadmap order

1. Finish Phase 11 Android and iOS app MVP.
2. Start Phase 12 Functional and Load Evidence tools.
3. Start Phase 13 GARP M 2026 change integration.
4. Start Phase 14 production hardening and release readiness.
5. Start Phase 15 app store and mobile release preparation.
6. Start Phase 16 provider/physician portal expansion.
7. Start Phase 17 capstone/RPL portfolio evidence export.

## Immediate next task

Phase 11 code is complete. Remaining Phase 11 work needs real hardware / a Mac:
run docs/phase-11/PHASE_11_MOBILE_SMOKE_TEST_CHECKLIST.md end to end on a
physical Android device, build and test on iOS from a Mac, then mark the
Phase 11 milestone complete.

After that, start Phase 12 - Functional and Load Evidence tools.

## Project rules to preserve

- Preparation support only.
- Do not provide legal advice.
- Do not provide medical advice.
- Do not calculate impairment points as a decision outcome.
- Do not estimate compensation as a guaranteed amount.
- Do not make or imply DVA decisions.
- Do not guarantee claim outcomes.
- Keep source references versioned and auditable.
- Keep admin write actions auditable.
- Do not store private service account keys, database connection strings or backend secrets in the mobile app.
