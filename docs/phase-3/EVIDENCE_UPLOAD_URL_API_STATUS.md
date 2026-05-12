# Evidence Upload URL API

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Evidence upload URL API has been implemented.

## Endpoints

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-upload-url

POST /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/mark-uploaded

POST /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/download-url

## Behaviour

The upload URL endpoint creates an evidence metadata record and returns a temporary signed Cloud Storage PUT URL.

The mark-uploaded endpoint confirms the object exists in Cloud Storage and updates the evidence item status to UPLOADED.

The download-url endpoint returns a temporary signed Cloud Storage GET URL for an existing evidence item.

## Security behaviour

All endpoints require a valid Firebase bearer token.

Without a token, endpoints return 401 Unauthorized.

## Bucket

dva-sop-dev-vapi-dev-evidence

## Safety note

This API enables direct-to-bucket upload but does not make the bucket public.

Signed URLs are temporary.

Future work should add file type validation, size limits, malware scanning and document-processing workflow before handling real veteran evidence.

## Next task

Build evidence gap tracker API.
