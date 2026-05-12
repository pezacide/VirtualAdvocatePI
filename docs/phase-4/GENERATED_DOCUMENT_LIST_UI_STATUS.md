# Generated Document List UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The generated document list UI has been connected to the backend generated document metadata API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/GeneratedDocumentListPanel.tsx

web/src/app/claim-workspaces/[workspaceId]/generated-documents/page.tsx

## Backend endpoints used

GET /api/v1/claim-workspaces/{workspaceId}/generated-documents

POST /api/v1/claim-workspaces/{workspaceId}/generated-documents

PATCH /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}

## Current behaviour

Signed-in users can open generated documents for a real workspace.

The page can load generated document metadata records.

The page can create generated document metadata records.

The page can update document status, template version, storage paths and included AI draft IDs.

The page displays summary counts for total, requested, generated, downloaded and failed records.

## Current limitation

This is metadata UI only.

It does not generate DOCX or PDF files yet.

It does not provide file download links yet.

## Safety note

Generated document records are preparation metadata only at this stage.

The page does not submit material to DVA, provide legal advice, provide medical advice, estimate compensation, or guarantee claim success.

## Next task

Add protected route handling.