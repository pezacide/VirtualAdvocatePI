# Firebase Token Verification and User Service

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

Firebase token verification has been implemented in the API.

## Endpoint

/api/v1/me

## Local test result

Without a Firebase bearer token, the endpoint returns:

401 Unauthorized

## Cloud Run expected result

Without a Firebase bearer token, the deployed endpoint should also return:

401 Unauthorized

## Behaviour with valid Firebase token

When a valid Firebase ID token is supplied later by the web app, the API will:

1. Verify the Firebase token.
2. Read the Firebase UID.
3. Create or update the matching local AppUser record.
4. Return the current user profile.

## Database table used

users

## Safety note

Firebase Authentication confirms identity only.

All future claim, evidence, AI, and document endpoints must also enforce user-level and workspace-level access control.

## Next task

Build claim workspace CRUD services.
