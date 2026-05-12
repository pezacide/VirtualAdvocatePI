# Condition and Accepted-Condition History APIs

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Condition intake and accepted-condition history API endpoints have been implemented.

## Condition endpoints

GET /api/v1/claim-workspaces/{workspaceId}/conditions

POST /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}

## Accepted-condition history endpoints

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history/{historyId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history/{historyId}

## Security behaviour

All endpoints require a valid Firebase bearer token.

Without a token, endpoints return 401 Unauthorized.

## Data tables

claim_conditions

accepted_condition_history

## Safety note

The accepted-condition history API captures previous DVA acceptance and compensation context only.

It does not calculate a baseline impairment rating, GARP M score, compensation amount, or DVA outcome.

## Next task

Build guided question response API.
