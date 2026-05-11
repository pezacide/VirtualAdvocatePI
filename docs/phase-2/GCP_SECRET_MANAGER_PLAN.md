# Virtual Advocate PI - Secret Manager Baseline Plan

## Google Cloud project

dva-sop-dev

## Environment

dev

## Secret naming rule

All development secrets will start with:

vapi-dev-

## Initial placeholder secrets

vapi-dev-db-connection-string

Purpose:
Placeholder for the future Cloud SQL PostgreSQL connection string.

Access:
vapi-dev-api
vapi-dev-docgen

vapi-dev-app-settings

Purpose:
Placeholder for general backend runtime settings that should not be hard-coded.

Access:
vapi-dev-api

vapi-dev-ai-settings

Purpose:
Placeholder for AI orchestration settings.

Access:
vapi-dev-aiworker

vapi-dev-docai-processor-id

Purpose:
Placeholder for future Document AI processor ID.

Access:
vapi-dev-aiworker

vapi-dev-jwt-signing-key

Purpose:
Placeholder only. Future backend signing or token-related secret if needed.

Access:
vapi-dev-api

## Safety rule

Do not store real production secrets yet.

Do not store real veteran health, claim, identity, or evidence data in any secret.

Only grant service accounts access to the specific secrets they need.

Prefer secret-level IAM bindings instead of broad project-level Secret Manager access.

## Next step

Create placeholder secrets and bind access to the relevant dev service accounts.
