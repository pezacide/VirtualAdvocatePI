# Tomorrow Start Here

## App

Virtual Advocate PI

## Current phase

Phase 3 - Core backend and database

## Current status

The backend API has been created and deployed to Cloud Run.

Cloud SQL PostgreSQL has been provisioned.

The initial EF Core migration has been applied successfully.

Firebase token verification has been added.

The /api/v1/me endpoint returns 401 without a Firebase bearer token.

The Claim Workspace API has been added.

The local /api/v1/claim-workspaces endpoint returns 401 without a Firebase bearer token, which is correct.

## First task tomorrow

Redeploy the updated API to Cloud Run and confirm the deployed Claim Workspace API is protected.

Run:

cd C:\Projects\VirtualAdvocatePI

.\scripts\gcp\deploy-dev-api-cloudbuild.ps1

Then test:

$ServiceUrl = gcloud run services describe vapi-dev-api --region=australia-southeast1 --project=dva-sop-dev --format="value(status.url)"

Invoke-RestMethod "$ServiceUrl/health"

Invoke-WebRequest "$ServiceUrl/api/v1/claim-workspaces"

Expected result for /api/v1/claim-workspaces without token:

401 Unauthorized

## After that

Mark this ProjectLibre task as complete:

Build claim workspace API (CRUD)

Then start:

Build condition intake API
