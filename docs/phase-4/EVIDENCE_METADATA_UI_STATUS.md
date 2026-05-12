# Evidence Metadata UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The evidence metadata UI has been connected to the backend evidence metadata API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/EvidenceMetadataPanel.tsx

web/src/app/claim-workspaces/[workspaceId]/evidence-metadata/page.tsx

## Backend endpoints used

GET /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

## Current behaviour

Signed-in users can open evidence metadata for a real workspace.

The page loads existing conditions for the workspace.

The page lets the user select a condition.

The page records evidence type, evidence status, file/document name, file type, document date, provider/source, notes and whether the item should be used in a generated pack.

The page displays saved evidence metadata records.

## Safety note

The evidence metadata page stores preparation information only.

It does not upload files yet, submit material to DVA, provide legal advice, provide medical advice, estimate compensation, or guarantee claim success.

## Next task

Build evidence upload UI.