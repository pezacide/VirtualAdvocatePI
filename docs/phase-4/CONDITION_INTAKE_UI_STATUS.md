# Condition Intake UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The condition intake UI has been connected to the backend condition API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/ConditionIntakePanel.tsx

web/src/app/claim-workspaces/[workspaceId]/conditions/page.tsx

## Backend endpoints used

GET /api/v1/claim-workspaces/{workspaceId}/conditions

POST /api/v1/claim-workspaces/{workspaceId}/conditions

## Current behaviour

Signed-in users can open a condition intake route for a real workspace.

The page sends the Firebase ID token to the backend API.

The page can list existing conditions for the workspace.

The page can add a new condition with diagnosis status, symptoms, treatment, medication and functional impact notes.

## Safety note

The condition intake page helps organise user-provided information only.

It does not diagnose a condition, provide medical advice, submit material to DVA, estimate compensation, or guarantee a claim outcome.

## Next task

Build accepted-condition history UI.