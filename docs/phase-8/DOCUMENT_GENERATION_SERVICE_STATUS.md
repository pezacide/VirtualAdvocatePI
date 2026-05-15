# Document Generation Service Status

## App

Virtual Advocate PI

## Phase

Phase 8 - Claim Starter Pack document generation

## Task

Build document generation service

## Status

Completed.

## Backend endpoint created

POST /api/v1/claim-workspaces/{workspaceId}/generated-documents/claim-starter-pack.

## Backend file created

backend/src/VirtualAdvocatePI.Api/Features/Documents/ClaimStarterPackDocumentEndpoints.cs.

## Package added

DocumentFormat.OpenXml.

## Behaviour

The endpoint gathers active workspace data.

The endpoint gathers active conditions.

The endpoint gathers active accepted-condition history.

The endpoint gathers active question responses.

The endpoint gathers active evidence metadata.

The endpoint gathers active evidence gaps.

The endpoint includes approved AI drafts only.

The endpoint generates a DOCX Claim Starter Pack in memory.

The endpoint uploads the DOCX to Google Cloud Storage.

The endpoint creates a GeneratedDocument metadata record.

The endpoint stores DocxStoragePath.

The endpoint records GENERATED_DOCUMENT_CREATED.

The endpoint records CLAIM_STARTER_PACK_DOCX_GENERATED.

## Build repair

StorageClient.CreateAsync was corrected to use the no-argument call pattern used elsewhere in the backend.

Backend build now passes.

## Safety boundary

The generated pack is preparation support only.

It does not submit anything to DVA.

It does not provide legal advice.

It does not provide medical advice.

It does not make DVA decisions.

It does not calculate impairment points.

It does not estimate compensation.

It does not guarantee claim outcomes.

## Next task

Add PDF conversion and storage versioning.
