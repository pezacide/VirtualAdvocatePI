# Guided Question Response UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The guided question response UI has been connected to the backend question response API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/GuidedQuestionResponsePanel.tsx

web/src/app/claim-workspaces/[workspaceId]/guided-questions/page.tsx

## Backend endpoints used

GET /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

## Current behaviour

Signed-in users can open guided questions for a real workspace.

The page loads existing conditions for the workspace.

The page lets the user select a condition.

The page records a question group, question key, question text, answer type and plain-English answer.

The page displays saved guided question responses.

## Safety note

The guided question response page stores user-provided preparation information only.

It does not provide legal advice, medical advice, a DVA decision, a compensation estimate, or a guarantee of claim success.

## Next task

Build evidence checklist shell.