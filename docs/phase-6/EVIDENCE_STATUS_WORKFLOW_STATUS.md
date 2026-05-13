# Evidence Status Workflow Status

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Build evidence status workflow

## Status

Completed.

## Files added or updated

web/src/lib/api/evidence.ts

web/src/lib/evidenceUi.ts

web/src/components/EvidenceUploadPanel.tsx

web/src/components/EvidenceMetadataPanel.tsx

## Current behaviour

Evidence cards show friendly status labels.

Users can update evidence preparation status from the evidence upload page.

Users can update evidence preparation status from the evidence metadata page.

Supported statuses are Missing, Listed not uploaded, Uploaded, Reviewed, Confirmed and Not applicable.

Evidence status reloads after refresh.

Open file action remains available for uploaded evidence.

## Backend alignment

The workflow uses the existing evidence item PATCH endpoint.

No new backend status endpoint was added.

Status values are aligned to the backend allowed values.

## Safety boundary

Evidence status is user preparation status only.

It does not mean DVA has reviewed, accepted or relied on the evidence.

It does not decide whether evidence is sufficient, prove service connection, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Build evidence list and detail view improvements.
