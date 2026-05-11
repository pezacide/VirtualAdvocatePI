# Google Cloud Enabled Services

## App

Virtual Advocate PI

## Google Cloud project

dva-sop-dev

## Environment strategy

Virtual Advocate PI is currently using one shared MVP Google Cloud project because new project creation is blocked by project quota.

Logical resource prefixes will be used:

dev  = vapi-dev
test = vapi-test
prod = vapi-prod

## Enabled core services

Cloud Run
Cloud Build
Artifact Registry
Cloud SQL Admin
Cloud Storage
Secret Manager
Vertex AI
Document AI
Cloud Logging
Cloud Monitoring
IAM
IAM Credentials
Cloud Resource Manager
Service Usage
Identity Toolkit / Firebase Auth support
Firebase Management
Firebase App Check
Pub/Sub
Cloud Tasks

## Safety note

Do not store real veteran health, claim, identity, or evidence data until privacy, consent, access control, deletion, audit logging, and storage security controls are ready.

## Next Phase 2 task

Create baseline IAM/service account plan.
