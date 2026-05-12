# Virtual Advocate PI Phase 3 API Documentation

## Purpose

This document describes the Phase 3 backend API foundation for Virtual Advocate PI.

The Phase 3 API supports claim workspace setup, condition intake, accepted-condition history, guided question responses, evidence metadata, audit events, evidence upload URL metadata, evidence gap tracking, AI draft metadata, and generated document metadata.

## Safety boundary

The API supports evidence and document preparation only.

It does not calculate GARP impairment points.

It does not estimate compensation.

It does not predict DVA outcomes.

It does not provide legal advice.

It does not provide medical advice.

It does not replace a doctor, advocate, lawyer, or DVA decision-maker.

## Public endpoints

GET /health

GET /api/v1/build-info

GET /api/v1/config/secret-health

GET /api/v1/db/health

GET /api/v1/db/schema-health

## Protected endpoints

All user, claim, condition, evidence, AI draft, and generated document endpoints require a Firebase bearer token.

Without a bearer token, these endpoints should return:

401 Unauthorized

## Core protected endpoint groups

Current user:

GET /api/v1/me

Claim workspaces:

GET /api/v1/claim-workspaces

POST /api/v1/claim-workspaces

GET /api/v1/claim-workspaces/{workspaceId}

PATCH /api/v1/claim-workspaces/{workspaceId}

DELETE /api/v1/claim-workspaces/{workspaceId}

Conditions:

GET /api/v1/claim-workspaces/{workspaceId}/conditions

POST /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}

Accepted-condition history:

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history/{historyId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history/{historyId}

Guided question responses:

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{responseId}

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{responseId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{responseId}

Evidence metadata:

GET /api/v1/claim-workspaces/{workspaceId}/evidence-items

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items

GET /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}

PATCH /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}

DELETE /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}

Evidence upload URL:

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-upload-url

POST /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/mark-uploaded

POST /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/download-url

Evidence gaps:

GET /api/v1/claim-workspaces/{workspaceId}/evidence-gaps

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/recalculate

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/{gapId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/{gapId}

Audit events:

GET /api/v1/claim-workspaces/{workspaceId}/audit-events

GET /api/v1/claim-workspaces/{workspaceId}/audit-events/{auditEventId}

AI draft metadata:

GET /api/v1/claim-workspaces/{workspaceId}/ai-drafts

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/ai-drafts

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts

GET /api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}

PATCH /api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}

DELETE /api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}

Generated document metadata:

GET /api/v1/claim-workspaces/{workspaceId}/generated-documents

POST /api/v1/claim-workspaces/{workspaceId}/generated-documents

GET /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}

PATCH /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}

DELETE /api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}

## Test coverage added

A backend test project has been added at:

backend/tests/VirtualAdvocatePI.Api.Tests

The first tests check that:

/health returns OK

/api/v1/build-info returns OK

Protected endpoints return 401 Unauthorized without a Firebase bearer token

## Smoke test script

The deployed dev API can be checked with:

scripts/api/smoke-test-dev-api.ps1
