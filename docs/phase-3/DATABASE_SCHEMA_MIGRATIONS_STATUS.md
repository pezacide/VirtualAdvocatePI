# Database Schema and Migrations Status

## App

Virtual Advocate PI

## Phase

Phase 3 - Core backend and database

## Google Cloud project

dva-sop-dev

## Cloud SQL instance

vapi-dev-postgres

## Database

vapi_dev

## Migration status

Initial Entity Framework Core migration has been created and applied successfully.

## Current schema includes

users

claim_workspaces

claim_conditions

EF Core migration history table

## Local migration method

Local migrations are applied through Cloud SQL Auth Proxy using:

Host=127.0.0.1
Port=5432
Database=vapi_dev
Username=vapi_app

## Cloud Run connection method

Cloud Run uses the Secret Manager secret:

vapi-dev-db-connection-string

and the Cloud SQL instance attachment:

dva-sop-dev:australia-southeast1:vapi-dev-postgres

## Safety note

The database schema is for development only at this stage.

Do not store real veteran health, claim, identity, or evidence data until privacy, consent, access control, deletion, audit logging, and security controls are complete.

## Next task

Implement Firebase token verification and user service.
