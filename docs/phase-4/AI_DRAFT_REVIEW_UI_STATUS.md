# AI Draft Review UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The AI draft review UI has been connected to the backend AI draft metadata API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/AiDraftReviewPanel.tsx

web/src/app/claim-workspaces/[workspaceId]/ai-drafts/page.tsx

## Backend endpoints used

GET /api/v1/claim-workspaces/{workspaceId}/ai-drafts

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/ai-drafts

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts

PATCH /api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}

GET /api/v1/claim-workspaces/{workspaceId}/conditions

## Current behaviour

Signed-in users can open AI draft review for a real workspace.

The page can load workspace-level AI draft metadata.

The page can load condition-level AI draft metadata.

The page can create manual draft metadata records.

The page can edit draft text and user-edited text.

The page can update review status, including approved, rejected and user-edited states.

## Current limitation

This is not live AI generation yet.

The page stores draft metadata and review state only.

## Safety note

Drafts are preparation text only and must be reviewed before use.

The page does not provide legal advice, medical advice, a DVA decision, a compensation estimate, or a guarantee of claim success.

## Next task

Build generated document list UI.