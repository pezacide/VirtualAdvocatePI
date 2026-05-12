# Evidence Gap Tracker UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The evidence gap tracker UI has been connected to the backend evidence gap API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/EvidenceGapTrackerPanel.tsx

web/src/app/claim-workspaces/[workspaceId]/evidence-gaps/page.tsx

## Backend endpoints used

GET /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/evidence-gaps

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/recalculate

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/{gapId}

## Current behaviour

Signed-in users can open the evidence gap tracker for a real workspace.

The page loads existing conditions for the workspace.

The page lets the user select a condition.

The page can recalculate evidence gaps for the selected condition.

The page displays gap type, severity, status, explanation and suggested next step.

The page lets the user update the gap status.

## Safety note

Evidence gaps are preparation prompts only.

They do not tell the user what DVA will require, provide legal advice, provide medical advice, estimate compensation, or guarantee claim success.

## Next task

Build AI draft review UI.