# Evidence Gap Tracker API

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Evidence gap tracker API endpoints have been implemented.

## Endpoints

GET /api/v1/claim-workspaces/{workspaceId}/evidence-gaps

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/recalculate

PATCH /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/{gapId}

DELETE /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/{gapId}

## Data table

evidence_gaps

## Gap examples

DIAGNOSIS_EVIDENCE_MISSING

CURRENT_TREATMENT_EVIDENCE_MISSING

MEDICATION_EVIDENCE_MISSING

FUNCTIONAL_IMPACT_NOTES_MISSING

PREVIOUS_DVA_DECISION_LETTER_MISSING

PREVIOUS_ASSESSMENT_LETTER_MISSING

WORSENING_EVIDENCE_MISSING

## Security behaviour

All endpoints require a valid Firebase bearer token.

Without a token, endpoints return 401 Unauthorized.

## Safety note

The evidence gap tracker provides plain-English preparation prompts only.

It does not calculate GARP M impairment points, estimate compensation, predict DVA outcomes, provide legal advice, provide medical advice, or say that missing evidence guarantees refusal or uploaded evidence guarantees success.

## Next task

Build AI draft metadata API.
