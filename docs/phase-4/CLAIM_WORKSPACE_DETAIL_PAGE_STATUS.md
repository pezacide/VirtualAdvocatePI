# Claim Workspace Detail Page

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The claim workspace detail page has been connected to the backend claim workspace API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/ClaimWorkspaceDetailPanel.tsx

web/src/app/claim-workspaces/[workspaceId]/page.tsx

## Backend endpoint used

GET /api/v1/claim-workspaces/{workspaceId}

## Current behaviour

Signed-in users can open a real claim workspace from the dashboard.

The page sends the Firebase ID token to the backend API.

The page displays workspace title, claim scenario, framework, status, generated pack status and workspace ID.

The page displays section cards for condition intake, accepted-condition history, guided questions, evidence checklist, evidence gaps, AI drafts and generated documents.

## Safety note

The workspace detail page is a preparation workspace only.

It does not create a DVA claim, submit material to DVA, provide legal advice, provide medical advice, estimate compensation, or guarantee a claim outcome.

## Next task

Build condition intake UI.