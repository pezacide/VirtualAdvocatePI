# Cloud Run API Deployment Status

## App

Virtual Advocate PI

## Google Cloud project

dva-sop-dev

## Region

australia-southeast1

## Cloud Run service

vapi-dev-api

## Service URL

https://vapi-dev-api-2pwcdyx42q-ts.a.run.app

## Health endpoint

https://vapi-dev-api-2pwcdyx42q-ts.a.run.app/health

## Current status

Deployed and responding successfully.

## Test result

/health returns:

healthy
vapi-dev-api
Virtual Advocate PI

## Security note

This skeleton API is currently publicly reachable for health-check testing only.

Before real user, claim, evidence, AI, or document endpoints are added, the backend must verify Firebase Authentication tokens and enforce user/workspace access control.

## Next task

Provision Cloud SQL PostgreSQL and Cloud Storage buckets.
