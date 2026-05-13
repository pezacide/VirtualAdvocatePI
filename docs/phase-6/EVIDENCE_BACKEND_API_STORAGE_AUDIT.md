# Evidence Backend API and Storage Flow Audit

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Audit backend evidence API and storage flow

## Purpose

This audit records the existing backend evidence endpoints, storage flow, evidence models and validation values before Phase 6 upload, tagging, status and gap tracker work continues.

## Backend areas audited

Evidence item endpoints

Evidence upload URL endpoint

Evidence mark-uploaded endpoint

Evidence download URL endpoint

Evidence gap endpoints

Evidence item model

Evidence gap model

Storage, upload, download and URL generation references

Evidence validation values

## Raw audit outputs

docs/phase-6/audit/EVIDENCE_BACKEND_ENDPOINT_AUDIT_RAW.txt

docs/phase-6/audit/EVIDENCE_STORAGE_FLOW_AUDIT_RAW.txt

docs/phase-6/audit/EVIDENCE_BACKEND_MODEL_AUDIT_RAW.txt

docs/phase-6/audit/EVIDENCE_BACKEND_VALIDATION_AUDIT_RAW.txt

## Initial backend questions to answer from the raw audit

Which evidence endpoints are already implemented?

Which endpoints are placeholders versus fully working flows?

Which storage provider is currently active?

Does the backend generate upload URLs?

Does the backend generate download or view URLs?

Does the backend record file name, content type, storage key and uploaded timestamp?

Does the backend link evidence to workspace ID and condition ID?

Does the backend support evidence status changes?

Does the backend support evidence gap recalculation?

Does the backend support gap status updates?

## Phase 6 design notes

The next implementation task should use the backend audit results to decide whether to improve an existing upload and download flow or build missing storage support.

Evidence upload work should preserve workspace ownership checks and condition ownership checks.

Evidence download or view actions should avoid exposing files without an authenticated, authorised request.

Evidence status values should be aligned between backend and web UI before adding new status workflows.

Evidence gap values should be aligned between backend and web UI before adding new gap rules.

## Safety boundary

Evidence tooling supports preparation only.

It does not submit material to DVA, decide whether evidence is sufficient, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Build signed upload and download URL flow.
