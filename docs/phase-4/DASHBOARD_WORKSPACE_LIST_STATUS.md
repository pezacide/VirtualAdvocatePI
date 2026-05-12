# Dashboard and Claim Workspace List

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The dashboard now loads claim workspaces from the backend API using the signed-in Firebase user session.

## Files added or updated

web/src/lib/apiClient.ts

web/src/components/DashboardWorkspaceList.tsx

web/src/app/dashboard/page.tsx

backend/src/VirtualAdvocatePI.Api/Program.cs

## Backend behaviour

The backend now allows local web app CORS access from:

http://localhost:3000

## Web behaviour

The dashboard detects Firebase session state.

The dashboard gets a Firebase ID token.

The dashboard calls:

GET /api/v1/claim-workspaces

The dashboard displays an empty state when no workspaces exist.

The dashboard displays workspace cards when workspaces exist.

## Safety note

The dashboard is a claim preparation workspace list only.

It does not create a DVA claim, submit material to DVA, provide legal advice, provide medical advice, estimate compensation, or guarantee a claim outcome.

## Next task

Build new claim and pathway selector.