# Cloud Run API Cloud SQL and Secret Manager Connection

## App

Virtual Advocate PI

## Google Cloud project

dva-sop-dev

## Cloud Run service

vapi-dev-api

## Cloud SQL instance

vapi-dev-postgres

## Database

vapi_dev

## Secret used

vapi-dev-db-connection-string

## Runtime environment variable

DATABASE_CONNECTION_STRING

## Test endpoints

/health

/api/v1/config/secret-health

/api/v1/db/health

## Status

Cloud Run API is connected to Secret Manager and Cloud SQL.

## Security note

The database connection string is not displayed by the API.

The current public health/test endpoints are for MVP infrastructure validation only.

Before real claim, evidence, AI, or user endpoints are added, Firebase Authentication token verification and user/workspace access control must be implemented.
