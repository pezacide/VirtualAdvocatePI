# Login and Session UI

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Status

Login and session UI has been improved.

## Files added or updated

web/src/components/AppHeader.tsx

web/src/components/AuthStatusPanel.tsx

web/src/app/dashboard/page.tsx

## Behaviour

The web app now has a reusable header.

The header shows session state.

Signed-in users can sign out from the header.

The dashboard displays a Firebase session status panel.

The login and session-check pages remain available for testing authentication.

## Safety note

Authentication confirms app session identity only.

It does not create a DVA claim, submit material to DVA, provide legal advice, provide medical advice, or guarantee a claim outcome.

## Next task

Build dashboard and claim workspace list.