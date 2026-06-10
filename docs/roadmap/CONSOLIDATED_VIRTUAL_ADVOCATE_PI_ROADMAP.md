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

Completed so far:

- MAUI app shell and shared services.
- Mobile API environment settings.
- Firebase Authentication.
- Authenticated API client and token flow.

Still to complete:

- Build dashboard and claim workspace screens.
- Build condition intake and question engine screens.
- Build evidence checklist and upload flow.
- Build AI draft review and document download screens.
- Add mobile loading, error and empty states.
- Add Android app icon, name and basic release config.
- Add iOS app icon, name and basic release config.
- Complete Android device testing.
- Complete iOS device testing.
- Add Phase 11 mobile smoke test checklist.
- Milestone: Phase 11 complete.

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

### GARP M 2026 proposed implementation phase

Recommended new phase:

Phase 12 - GARP M 2026 change integration

Tasks:

- Import and register GARP M 2026 source documents.
- Complete current-versus-July-2026 GARP M comparison note.
- Add GARP M version metadata to source registry and templates.
- Update GARP M question engine content.
- Update GARP M summary content.
- Update evidence checklist mappings.
- Update Claim Starter Pack template wording.
- Update Doctor Guidance Pack template wording.
- Add GARP M 2026 prompt/disclaimer versions.
- Add admin audit review filters for GARP M 2026 events.
- Add GARP M 2026 smoke test checklist.
- Milestone: GARP M 2026 integration complete.

## Recommended next roadmap order

1. Finish Phase 11 Android and iOS app MVP.
2. Start Phase 12 GARP M 2026 change integration.
3. Start Phase 13 production hardening and release readiness.
4. Start Phase 14 app store and mobile release preparation.
5. Start Phase 15 provider/physician portal expansion.
6. Start Phase 16 capstone/RPL portfolio evidence export.

## Immediate next task

Continue Phase 11:

Build dashboard and claim workspace screens.

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
