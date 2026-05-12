# Accepted-Condition History UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The accepted-condition history UI has been connected to the backend accepted-condition history API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/AcceptedConditionHistoryPanel.tsx

web/src/app/claim-workspaces/[workspaceId]/accepted-history/page.tsx

## Backend endpoints used

GET /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history

## Current behaviour

Signed-in users can open accepted-condition history for a real workspace.

The page loads existing conditions for the workspace.

The page lets the user select a condition.

The page records previous DVA acceptance, original Act, previous compensation, decision letter availability, assessment letter availability and worsening information.

The page displays saved accepted-condition history records.

## Safety note

The accepted-condition history page stores user-provided preparation information only.

It does not confirm DVA acceptance, provide legal advice, provide medical advice, estimate compensation, or guarantee a claim outcome.

## Next task

Build guided question response UI.