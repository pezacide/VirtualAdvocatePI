# New Claim and Pathway Selector

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The new claim and pathway selector UI has been connected to the backend claim workspace API.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/NewClaimPathwaySelector.tsx

web/src/app/claim-workspaces/new/page.tsx

## Backend endpoint used

POST /api/v1/claim-workspaces

## Supported pathway options

NEW_CONDITION

WORSENING_EXISTING_CONDITION

NEW_PLUS_EXISTING

EVIDENCE_PREP_ONLY

UNSURE

## Current behaviour

Signed-in users can create a claim preparation workspace.

The page sends the Firebase ID token to the backend API.

After successful creation, the user is redirected to the workspace detail route.

The dashboard can display the created workspace.

## Safety note

Creating a workspace only creates an app preparation workspace.

It does not create a DVA claim, submit material to DVA, provide legal advice, provide medical advice, estimate compensation, or guarantee a claim outcome.

## Next task

Build claim workspace detail page.