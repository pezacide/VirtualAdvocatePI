# AI Draft Metadata API

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

AI draft metadata API endpoints have been implemented.

## Endpoints

GET /api/v1/claim-workspaces/{workspaceId}/ai-drafts

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/ai-drafts

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts

GET /api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}

PATCH /api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}

DELETE /api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}

## Data table

ai_drafts

## Supported draft types

VETERAN_STATEMENT

WORSENING_SUMMARY

EVIDENCE_GAP_SUMMARY

DOCTOR_APPOINTMENT_QUESTIONS

DOCTOR_REQUEST_LETTER

CLAIM_PACK_COVER_NOTE

## Review statuses

DRAFT_CREATED

USER_REVIEW_REQUIRED

USER_EDITED

APPROVED

REJECTED

REGENERATED

## Security behaviour

All endpoints require a valid Firebase bearer token.

Without a token, endpoints return 401 Unauthorized.

## Safety note

This API stores AI draft metadata and draft text only.

It does not call Vertex AI yet.

AI-generated content must remain draft-only until reviewed, edited, approved or rejected by the user.

## Next task

Build generated document metadata API.
