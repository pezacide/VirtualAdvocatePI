# Evidence Upload, Metadata and Gap Route Audit

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Audit existing evidence upload, metadata and gap routes

## Purpose

This audit records the existing evidence-related web routes, components and API client references before Phase 6 evidence upload and gap tracker work continues.

## Existing evidence-related web routes

/claim-workspaces/[workspaceId]/evidence-checklist

/claim-workspaces/[workspaceId]/evidence-metadata

/claim-workspaces/[workspaceId]/evidence-upload

/claim-workspaces/[workspaceId]/evidence-gaps

/claim-workspaces/[workspaceId]/accepted-history

/claim-workspaces/[workspaceId]/conditions

/claim-workspaces/[workspaceId]/garp-m-questions

/claim-workspaces/[workspaceId]/garp-m-summary

## Existing evidence API client functions found

getConditionEvidenceItems

createEvidenceItem

createEvidenceUploadUrl

markEvidenceUploaded

createEvidenceDownloadUrl

getWorkspaceEvidenceGaps

getConditionEvidenceGaps

recalculateEvidenceGaps

updateEvidenceGap

## Existing evidence API paths found

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-upload-url

POST /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/mark-uploaded

POST /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/download-url

GET /api/v1/claim-workspaces/{workspaceId}/evidence-gaps

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/recalculate

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/{gapId}

## Initial findings

The web app already has separate routes for evidence checklist, evidence metadata, evidence upload and evidence gaps.

The web app already has API client functions for evidence items, upload URLs, mark-uploaded confirmation, download URLs and evidence gaps.

The workspace tool navigation panel now gives users a clickable way to reach evidence tools.

The next task should inspect the backend endpoint implementations and storage behaviour before changing upload or download flows.

## Raw audit outputs

docs/phase-6/audit/EVIDENCE_WEB_ROUTE_AUDIT_RAW.txt

docs/phase-6/audit/EVIDENCE_WEB_API_CLIENT_AUDIT_RAW.txt

docs/phase-6/audit/EVIDENCE_ROUTE_FILES.txt

## Safety boundary

Evidence tooling supports preparation only.

It does not submit material to DVA, decide whether evidence is sufficient, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Audit backend evidence API and storage flow.
