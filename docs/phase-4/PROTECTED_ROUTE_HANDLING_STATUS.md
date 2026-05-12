# Protected Route Handling

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

Protected route handling has been added to the Next.js web app.

## Files added or updated

web/src/components/ProtectedRoute.tsx

web/src/app/dashboard/layout.tsx

web/src/app/claim-workspaces/layout.tsx

web/src/app/login/page.tsx

## Protected routes

/dashboard

/claim-workspaces/*

## Public routes

/

/login

/env-check

/session-check

## Current behaviour

Signed-out users are redirected to login before opening protected dashboard or workspace routes.

The login page supports a returnTo query parameter.

After sign-in, the user is returned to the protected route.

Signed-in users can continue directly to protected routes.

## Safety note

Route protection only controls app access.

It does not replace backend Firebase token verification.

The backend must continue to verify Firebase bearer tokens on protected API endpoints.

## Next task

Add API client service layer.