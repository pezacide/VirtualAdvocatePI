# Document Download Signed URL Flow Status

## App

Virtual Advocate PI

## Phase

Phase 8 - Claim Starter Pack document generation

## Task

Add document download signed URL flow

## Status

Completed.

## Backend endpoint created

POST /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}/download-url.

## Backend file created

backend/src/VirtualAdvocatePI.Api/Features/Documents/GeneratedDocumentDownloadEndpoints.cs.

## Frontend files updated

web/src/lib/api/generatedDocuments.ts.

web/src/components/GeneratedDocumentListPanel.tsx.

## Behaviour

The backend creates short-lived signed download URLs for generated DOCX and PDF files.

The backend reads the target object from DocxStoragePath or PdfStoragePath.

The backend updates DownloadedAt and DocumentStatus when a signed URL is created.

The backend records GENERATED_DOCUMENT_DOWNLOAD_URL_CREATED.

The frontend can generate a Claim Starter Pack.

The frontend can open DOCX and PDF download links.

## Safety boundary

Generated document downloads are preparation support only.

The download flow does not submit anything to DVA.

It does not provide legal advice.

It does not provide medical advice.

It does not make DVA decisions.

It does not calculate impairment points.

It does not estimate compensation.

It does not guarantee claim outcomes.

## Next task

Enforce reviewed-only content and test exports.
