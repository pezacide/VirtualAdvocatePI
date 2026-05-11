# Claim Workspace API CRUD Status

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Claim Workspace API CRUD endpoints have been implemented.

## Endpoints

GET /api/v1/claim-workspaces

POST /api/v1/claim-workspaces

GET /api/v1/claim-workspaces/{id}

PATCH /api/v1/claim-workspaces/{id}

DELETE /api/v1/claim-workspaces/{id}

## Security behaviour

All claim workspace endpoints require a valid Firebase bearer token.

Without a token, the endpoints return:

401 Unauthorized

## Data model

The API uses the claim_workspaces table.

## Claim framework

Every version 1 claim workspace uses:

IMPROVED_MRCA_POST_2026

## Supported claim scenarios

NEW_CONDITION

WORSENING_EXISTING_CONDITION

NEW_PLUS_EXISTING

EVIDENCE_PREP_ONLY

UNSURE

## Delete behaviour

DELETE is implemented as a soft archive.

The workspace status is set to:

ARCHIVED

## Safety note

The workspace API only allows access to workspaces belonging to the authenticated user.

Future endpoints must follow the same user/workspace ownership pattern.

## Next task

Build condition intake API.
