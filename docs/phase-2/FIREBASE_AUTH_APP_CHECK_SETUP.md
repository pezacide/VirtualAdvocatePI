# Firebase Authentication and App Check Setup

## App

Virtual Advocate PI

## Google Cloud / Firebase project

dva-sop-dev

## Web app

Virtual Advocate PI Dev Web

## Authentication

Firebase Authentication has been configured for the MVP setup pass.

MVP sign-in method:

Email/Password

Additional providers such as Google, Apple, phone, or social login are not part of the first setup pass.

## App Check

Firebase App Check has been registered for the web app.

Provider:

reCAPTCHA Enterprise

Initial allowed domain:

localhost

## Enforcement decision

App Check enforcement is disabled for MVP setup.

Reason:

The web app and backend token verification are not built yet. Enforcement should only be enabled after the app sends App Check tokens and the backend is ready to verify them.

## Local config files

config\firebase\firebase-web-dev.example.json

config\firebase\app-check-dev.example.json

## Safety note

Firebase Authentication confirms identity only.

The backend must still enforce access control so users can only access their own claim workspaces.

## Next task

Create Cloud Run API skeleton and CI/CD.
