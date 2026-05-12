# Guided Question Response API

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Guided question response API endpoints have been implemented.

## Endpoints

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{responseId}

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{responseId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{responseId}

## Security behaviour

All endpoints require a valid Firebase bearer token.

Without a token, endpoints return 401 Unauthorized.

## Data table

question_responses

## Supported question groups

CLAIM_CONTEXT
DIAGNOSIS
SYMPTOMS
TREATMENT
MEDICATION
FUNCTIONAL_IMPACT
LIFESTYLE_IMPACT
WORK_IMPACT
STABILITY
WORSENING
PREVIOUS_COMPENSATION
EVIDENCE_AVAILABLE
EVIDENCE_MISSING
DOCTOR_QUESTIONS

## Safety note

The guided question response API stores user-entered preparation information only.

It does not calculate GARP M impairment points, estimate compensation, predict DVA outcomes, provide legal advice, or provide medical advice.

## Next task

Build evidence metadata and audit APIs.
