# Web App Environment Variables

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

The web app environment variable structure has been configured and verified locally.

## Files added

web/.env.example

web/src/lib/env.ts

web/src/app/env-check/page.tsx

## Local-only file

web/.env.local

## Variables configured

NEXT_PUBLIC_API_BASE_URL

NEXT_PUBLIC_FIREBASE_API_KEY

NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN

NEXT_PUBLIC_FIREBASE_PROJECT_ID

NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET

NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID

NEXT_PUBLIC_FIREBASE_APP_ID

## Verified route

http://localhost:3000/env-check

## Safety note

Real local environment values must not be committed to Git.

The committed .env.example file contains placeholders only.

## Next task

Connect Firebase web authentication.