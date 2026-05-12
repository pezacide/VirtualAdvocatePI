# Generated Document Metadata API

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Generated document metadata API endpoints have been implemented.

## Endpoints

GET /api/v1/claim-workspaces/{workspaceId}/generated-documents

POST /api/v1/claim-workspaces/{workspaceId}/generated-documents

GET /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}

PATCH /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}

DELETE /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}

## Data table

generated_documents

## Supported document types

POST_2026_PI_CLAIM_STARTER_PACK

DOCTOR_GUIDANCE_PACK

DOCTOR_REQUEST_LETTER

EVIDENCE_GAP_SUMMARY

## Safety note

This API stores generated document metadata only.

It does not generate DOCX or PDF files yet.

Generated documents must clearly state that they are preparation documents only and are not legal advice, medical advice, financial advice, a DVA decision, a compensation estimate, or a guarantee of claim success.

## Next task

Refactor shared auth and ownership helpers.
