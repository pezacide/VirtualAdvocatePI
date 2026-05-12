# Evidence Upload UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The evidence upload UI has been connected to the backend signed upload URL flow.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/EvidenceUploadPanel.tsx

web/src/app/claim-workspaces/[workspaceId]/evidence-upload/page.tsx

config/gcp/vapi-dev-evidence-bucket-cors.json

## Backend endpoints used

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-upload-url

POST /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/mark-uploaded

GET /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

## Cloud Storage behaviour

The browser uploads directly to Cloud Storage using a short-lived signed upload URL.

The evidence bucket has local web CORS configured for localhost development.

## Current behaviour

Signed-in users can open evidence upload for a real workspace.

The page loads existing conditions for the workspace.

The page lets the user select a condition and evidence type.

The page creates a backend evidence item and signed upload URL.

The page uploads the selected file directly to Cloud Storage.

The page confirms the upload with the backend.

The page displays uploaded and listed evidence records.

## Safety note

Uploading evidence stores files for preparation use inside this app only.

It does not submit material to DVA, provide legal advice, provide medical advice, estimate compensation, or guarantee claim success.

## Next task

Build evidence gap tracker UI.