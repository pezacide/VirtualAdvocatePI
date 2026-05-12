# API Smoke Test Script

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

A PowerShell smoke test script has been created for the deployed dev API.

## Script

scripts/api/smoke-test-dev-api.ps1

## What it checks

/health returns 200

/api/v1/db/schema-health returns 200

Protected endpoints return 401 Unauthorized without a Firebase bearer token.

## Protected endpoint groups checked

Current user

Claim workspaces

Conditions

Accepted-condition history

Question responses

Evidence metadata

Audit events

Evidence upload URL

Evidence gaps

AI drafts

Generated document metadata

## Safety note

The smoke test does not create real veteran data.

It only checks public health endpoints and confirms protected endpoints reject unauthenticated requests.

## Next task

Redeploy and verify Phase 3 backend.
