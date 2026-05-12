# Firebase Web Authentication

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

Firebase web authentication has been connected to the Next.js web app.

## Files added or updated

web/src/lib/firebase.ts

web/src/components/AuthProvider.tsx

web/src/app/layout.tsx

web/src/app/login/page.tsx

web/src/app/session-check/page.tsx

## Authentication method

Email/password sign-in through Firebase Authentication.

## Verified routes

/login

/session-check

/env-check

## Current behaviour

The web app can register a Firebase user.

The web app can sign in with Firebase Authentication.

The web app can sign out.

The web app can detect session state.

The web app can retrieve a Firebase ID token preview for future backend API calls.

## Safety note

Authentication only confirms app session identity.

It does not create a DVA claim, submit material to DVA, provide legal advice, provide medical advice, or guarantee a claim outcome.

## Next task

Build login and session UI.