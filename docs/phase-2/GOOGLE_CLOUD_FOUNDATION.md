# Phase 2 - Google Cloud Foundation

## App name

Virtual Advocate PI

## Phase 2 objective

Create the Google Cloud foundation for the post-1 July 2026 improved MRCA / VETS Act-first PI Claim Starter Pack MVP.

## Environment structure

Virtual Advocate PI will use three Google Cloud projects:

1. Development
2. Test
3. Production

## Environment purpose

Development is for local development, experiments, early backend work, and non-sensitive test data.

Test is for pre-release validation, integration testing, and pilot-style testing before production.

Production is for the live app and must not be used for experiments.

## Important rule

Do not store real veteran health, claim, identity, or evidence information in development or test environments unless privacy, consent, security, access control, deletion, and audit logging are ready.

## Phase 2 first task

Create Google Cloud dev/test/prod project structure.
