# Mobile API Environment Settings

## App

Virtual Advocate PI

## Purpose

This document records the Phase 11 mobile API environment settings foundation.

## Current development API

https://vapi-dev-api-2pwcdyx42q-ts.a.run.app

## Health check path

/api/v1/config/secret-health

## Configuration file

maui/VirtualAdvocatePI.Mobile/Configuration/MobileAppSettings.cs.

## Behaviour

Debug builds use the Development environment.

Debug builds currently use mock authentication until Firebase Authentication is integrated.

Release builds are prepared to disable mock authentication.

The mobile home page displays the current environment name, API base URL and auth mode.

The mobile home page has a Check API connection button.

## Security note

Do not store secrets in the mobile app.

Firebase and API authentication will be integrated in later Phase 11 tasks.

Mobile app settings should contain public environment routing only, not private keys or database connection strings.

## Next task

Integrate Firebase Authentication.
