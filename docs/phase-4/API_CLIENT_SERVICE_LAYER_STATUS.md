# API Client Service Layer

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The web API client has been refactored into a service layer.

## Files added

web/src/lib/api/client.ts

web/src/lib/api/workspaces.ts

web/src/lib/api/conditions.ts

web/src/lib/api/acceptedHistory.ts

web/src/lib/api/questionResponses.ts

web/src/lib/api/evidence.ts

web/src/lib/api/evidenceGaps.ts

web/src/lib/api/aiDrafts.ts

web/src/lib/api/generatedDocuments.ts

web/src/lib/api/index.ts

## File removed

web/src/lib/apiClient.ts

## Behaviour

The web app now imports API methods from:

@/lib/api

The shared API client helper centralises:

API base URL handling

Authorization headers

JSON headers

GET requests

POST requests

PATCH requests

common API error handling

## Safety note

This is a refactor only.

It does not change backend authentication requirements.

Protected backend endpoints still require Firebase bearer token verification.

## Next task

Add web smoke test checklist.