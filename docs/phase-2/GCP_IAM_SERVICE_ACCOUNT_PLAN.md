# Virtual Advocate PI - Baseline IAM and Service Account Plan

## App

Virtual Advocate PI

## Google Cloud project

dva-sop-dev

## Current environment decision

Virtual Advocate PI is currently using one existing Google Cloud project because new project creation is blocked by project quota.

Logical environment prefixes will be used:

dev  = vapi-dev
test = vapi-test
prod = vapi-prod

For the MVP build, only the dev service accounts should be created first.

## IAM principle

Virtual Advocate PI will use separate service accounts for separate workloads.

The app should avoid using broad default service accounts where practical.

Each service account should only receive the minimum permissions needed for its job.

## Baseline service accounts

### 1. vapi-dev-api

Purpose:

Runtime identity for the main Cloud Run backend API.

Used by:

ASP.NET Core API on Cloud Run.

Likely needs access to:

Cloud SQL connection
Secret Manager secrets needed by the API
Cloud Storage evidence bucket
Vertex AI only if the API calls AI directly

Planned roles:

roles/cloudsql.client
roles/secretmanager.secretAccessor
bucket-level storage access, not broad project-wide storage access
roles/aiplatform.user only if the API calls Vertex AI directly

### 2. vapi-dev-aiworker

Purpose:

Runtime identity for the AI orchestration worker.

Used by:

Cloud Run service or job that prepares AI drafts, evidence gap summaries, doctor questions and claim pack text.

Likely needs access to:

Vertex AI
Secret Manager
Cloud Storage evidence or extracted-text bucket
Document AI if the same worker processes uploaded documents

Planned roles:

roles/aiplatform.user
roles/secretmanager.secretAccessor
roles/documentai.apiUser if it calls Document AI
bucket-level storage access, not broad project-wide storage access

### 3. vapi-dev-docgen

Purpose:

Runtime identity for the document generation service.

Used by:

Cloud Run service or job that generates DOCX/PDF claim packs.

Likely needs access to:

Cloud SQL metadata
Cloud Storage generated document bucket
Secret Manager template/config secrets if needed

Planned roles:

roles/cloudsql.client
roles/secretmanager.secretAccessor
bucket-level storage object create/read access

### 4. vapi-dev-build

Purpose:

Build/deploy identity for Cloud Build or CI/CD.

Used by:

Build pipeline that builds containers, pushes to Artifact Registry and deploys to Cloud Run.

Likely needs access to:

Artifact Registry
Cloud Run deployment
Permission to deploy using runtime service accounts

Planned roles:

roles/artifactregistry.writer
roles/run.admin
roles/iam.serviceAccountUser on only the runtime service accounts it deploys as

### 5. vapi-dev-adminops

Purpose:

Optional admin operations service account for controlled setup scripts.

Used by:

Infrastructure setup tasks only.

Likely needs access to:

Only the resources required during setup.

Planned roles:

Avoid broad Owner/Editor roles where possible.
Use only temporarily if needed.
Remove or reduce permissions after setup.

## Human access

The project owner/developer may need broad access during early MVP setup.

Before production, human access should be reduced and separated into:

Owner/admin
Developer
Support/admin user
Read-only auditor

## Storage access rule

Do not grant broad project-wide storage access unless unavoidable during early development.

Prefer bucket-level IAM for:

vapi-dev-evidence
vapi-dev-generated
vapi-dev-knowledge
vapi-dev-temp

Cloud Storage IAM roles can be applied at project or bucket level, so bucket-level access is preferred for sensitive evidence and generated claim documents.

## Secret access rule

Only runtime service accounts that need a secret should receive secret access.

Secret Manager is intended for sensitive data such as API keys, passwords and certificates, so access should be limited carefully.

## Cloud Run rule

Each Cloud Run service should run as its own workload-specific service account.

Cloud Run has predefined IAM roles for managing and accessing Cloud Run resources, but runtime permissions should be granted to the service account used by the service, not to all users.

## Document AI rule

Only the AI worker or document processing service should receive Document AI access.

Document AI IAM roles should not be granted to every service account.

## Vertex AI rule

Only the service account that calls Vertex AI/Gemini should receive Vertex AI access.

For MVP, this should normally be:

vapi-dev-aiworker

The main API should call the AI worker rather than every backend service calling Vertex AI directly.

## Production warning

Before public production launch, dev/test/prod should be split into separate Google Cloud projects once project quota allows.

Until then, production-like resources inside the shared project must not contain real veteran health, claim, identity or evidence data.

## Next implementation step

After this plan is reviewed, create the dev service accounts first.

Do not assign broad permissions until the storage buckets, Cloud SQL instance, Artifact Registry and Cloud Run services are created.
