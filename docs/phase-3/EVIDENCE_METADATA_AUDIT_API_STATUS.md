# Evidence Metadata and Audit APIs

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Evidence metadata and audit API endpoints have been implemented.

## Evidence metadata endpoints

GET /api/v1/claim-workspaces/{workspaceId}/evidence-items

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

GET /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}

PATCH /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}

DELETE /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}

## Audit endpoints

GET /api/v1/claim-workspaces/{workspaceId}/audit-events

GET /api/v1/claim-workspaces/{workspaceId}/audit-events/{auditEventId}

## Security behaviour

All endpoints require a valid Firebase bearer token.

Without a token, endpoints return 401 Unauthorized.

## Data tables

evidence_items

audit_events

## Safety note

The evidence metadata API stores evidence records and file references only.

Actual file upload, signed upload URLs and download URLs are not part of this task.

The audit API records important user and system actions but should not store unnecessary sensitive claim details.

## Next task

Build AI draft metadata API or evidence upload URL API.
