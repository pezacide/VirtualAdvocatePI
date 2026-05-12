# Phase 3 Backend Redeploy and Verification

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Status

The Phase 3 backend has been redeployed to Cloud Run and verified.

## Cloud Run service

vapi-dev-api

## Google Cloud project

dva-sop-dev

## Region

australia-southeast1

## Verification checks

/health returns healthy

/api/v1/db/schema-health confirms database connection

Protected endpoints return 401 Unauthorized without a Firebase bearer token

Smoke test script runs against the deployed dev API

## Smoke test script

scripts/api/smoke-test-dev-api.ps1

## Security note

Protected endpoints must return 401 without a Firebase bearer token.

A 401 result means the request reached the API and the API correctly rejected unauthenticated access.

## Next task

Add backend tests and API documentation.
