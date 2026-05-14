# Evidence Remove and Archive Button Status

## App

Virtual Advocate PI

## Phase

Phase 6.5 - Removal and archive controls

## Task

Build evidence remove/archive button

## Status

Completed.

## Backend finding

The backend already had a DELETE evidence item endpoint.

The endpoint archives evidence by setting Status to ARCHIVED.

Active evidence list endpoints already exclude archived evidence.

The backend already writes an EVIDENCE_ARCHIVED audit event.

No backend endpoint or database migration was required for this task.

## Files added or updated

web/src/lib/api/evidence.ts

web/src/lib/api/index.ts

web/src/components/EvidenceUploadPanel.tsx

web/src/components/EvidenceMetadataPanel.tsx

## Current behaviour

Evidence upload cards now include a Remove from workspace action.

Evidence metadata cards now include a Remove from workspace action.

The action asks for confirmation before removing evidence from the active workspace.

Removed evidence is archived rather than hard deleted.

Archived evidence disappears from active evidence lists after reload.

Archived evidence is excluded from active gap checks and future AI preparation because existing backend evidence queries exclude archived records.

The audit trail records the EVIDENCE_ARCHIVED event.

## Safety boundary

Removing evidence from the workspace does not contact DVA.

It does not remove anything already submitted outside this app.

It does not make a DVA decision, provide legal advice, provide medical advice, or guarantee any claim outcome.

## Build status

Backend build passed.

Web build passed.

## Next task

Build uploaded file deletion flow.
