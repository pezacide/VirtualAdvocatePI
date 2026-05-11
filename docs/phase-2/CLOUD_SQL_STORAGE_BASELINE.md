# Cloud SQL PostgreSQL and Cloud Storage Baseline

## App

Virtual Advocate PI

## Google Cloud project

dva-sop-dev

## Region

australia-southeast1

## Cloud SQL

Instance name:

vapi-dev-postgres

Database name:

vapi_dev

Database user:

vapi_app

Connection name:

dva-sop-dev:australia-southeast1:vapi-dev-postgres

Connection string secret:

vapi-dev-db-connection-string

## Cloud Storage buckets

Uploaded evidence:

gs://dva-sop-dev-vapi-dev-evidence

Generated documents:

gs://dva-sop-dev-vapi-dev-generated

Knowledge/source material:

gs://dva-sop-dev-vapi-dev-knowledge

Temporary processing files:

gs://dva-sop-dev-vapi-dev-temp

## Security model

Buckets use uniform bucket-level access.

Public access prevention is enabled.

Runtime service accounts have bucket-level access only.

The database connection string is stored in Secret Manager.

## Safety note

Do not upload real veteran health, claim, identity, or evidence data until privacy, consent, access control, deletion, audit logging, and storage security controls are complete.

## Next task

Connect Cloud Run API to Cloud SQL and Secret Manager.
