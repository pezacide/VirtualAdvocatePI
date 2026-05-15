# Doctor Guidance Pack Export Status

## App

Virtual Advocate PI

## Phase

Phase 9 - Doctor guidance pack

## Task

Export and test doctor guidance pack

## Status

Completed.

## Backend endpoint created

POST /api/v1/claim-workspaces/{workspaceId}/generated-documents/doctor-guidance-pack.

## Backend file created

backend/src/VirtualAdvocatePI.Api/Features/Documents/DoctorGuidancePackDocumentEndpoints.cs.

## Frontend files created

web/src/components/DoctorGuidanceExportPanel.tsx.

## Frontend files updated

web/src/lib/api/generatedDocuments.ts.

web/src/app/claim-workspaces/[workspaceId]/doctor-guidance/page.tsx.

## Export behaviour

The workflow generates a versioned DOCX Doctor Guidance Pack.

The workflow generates a versioned PDF Doctor Guidance Pack.

The workflow creates GeneratedDocument metadata.

The workflow reuses the existing generated-document signed download URL flow.

## Reviewed-only rule

Only approved doctor guidance AI drafts are included.

Unapproved, rejected, archived or still-reviewing doctor guidance drafts are excluded.

## Audit events

DOCTOR_GUIDANCE_PACK_CREATED.

DOCTOR_GUIDANCE_PACK_REVIEWED_ONLY_ENFORCED.

GENERATED_DOCUMENT_DOWNLOAD_URL_CREATED.

## Next task

Milestone: Phase 9 complete.
