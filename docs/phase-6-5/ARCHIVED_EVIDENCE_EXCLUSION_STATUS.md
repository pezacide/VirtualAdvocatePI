# Archived Evidence Exclusion Status

## App

Virtual Advocate PI

## Phase

Phase 6.5 - Removal and archive controls

## Task

Ensure archived evidence is excluded from gaps and AI drafts

## Status

Completed.

## Backend finding

Archived evidence items are already excluded from active evidence item list endpoints.

Evidence gap recalculation already filters evidence items with Status not equal to ARCHIVED.

Evidence gap recalculation also excludes evidence items marked MISSING or NOT_APPLICABLE.

Evidence gap recalculation already filters accepted-condition history and GARP M question responses with Status not equal to ARCHIVED.

AI draft metadata endpoints already exclude archived AI drafts.

Generated document metadata endpoints already exclude archived generated documents.

The inspected AI draft and generated document metadata endpoints do not directly pull archived evidence items as source material.

Uploaded-file deletion already clears StoragePath, UploadedAt and FileSize, returns the evidence item to LISTED_NOT_UPLOADED, and records EVIDENCE_UPLOADED_FILE_DELETED.

## Design note

Evidence gap rules currently treat listed evidence as useful evidence, not only uploaded evidence.

This matches the current plain-English gap wording of listed or uploaded evidence.

Deleting an uploaded file returns the evidence item to LISTED_NOT_UPLOADED, but the evidence item can still count as listed evidence unless the item itself is archived or removed from the workspace.

## Files inspected

backend/src/VirtualAdvocatePI.Api/Features/Evidence/EvidenceGapEndpoints.cs

backend/src/VirtualAdvocatePI.Api/Features/Ai/AiDraftEndpoints.cs

backend/src/VirtualAdvocatePI.Api/Features/Documents/GeneratedDocumentEndpoints.cs

backend/src/VirtualAdvocatePI.Api/Features/Evidence/EvidenceAndAuditEndpoints.cs

backend/src/VirtualAdvocatePI.Api/Features/Evidence/EvidenceUploadEndpoints.cs

## Code changes

No code patch was required for this task.

## Safety boundary

Archived evidence is excluded from active preparation workflows inside this app.

This does not contact DVA.

It does not remove anything already submitted outside this app.

It does not make a DVA decision, provide legal advice, provide medical advice, or guarantee any claim outcome.

## Next task

Run Phase 6.5 smoke test and close Phase 6.5.
