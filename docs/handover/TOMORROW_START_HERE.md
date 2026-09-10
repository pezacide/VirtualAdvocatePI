# Tomorrow Start Here

## App

Virtual Advocate PI

## Current phase

Phase 11 - Android and iOS app MVP (code complete)

## Current status

All Phase 11 mobile screens are built and the Android build (Debug and signed
Release `.aab`) and the Windows build pass with 0 warnings / 0 errors.

Screens: dashboard, disclaimer gate, new workspace, workspace detail, condition
intake, GARP M question engine, GARP M structured summary, Evidence (checklist +
per-condition items + file upload + gaps), AI draft review, and Generated
documents (generate + download).

App identity is set: name "Virtual Advocate PI", id
`au.com.virtualadvocatepi.mobile`, brand icon/splash, Android Release packaging.

The Release APK was installed on the `Pixel_10_Pro_XL` emulator and launches
cleanly to the Sign in screen (no startup crash).

No backend changes were needed.

## First task

Run `docs/phase-11/PHASE_11_MOBILE_SMOKE_TEST_CHECKLIST.md` end to end on a
physical Android device, signed in with a real Firebase test account against the
dev backend. Work through the Evidence, AI drafts and Generated documents
sections in particular - those flows could not be exercised without credentials
in the build environment.

## After that

1. Build and test on iOS from a Mac (`dotnet build -f net10.0-ios`); check the
   display name, icon and the same checklist. iOS config in
   `Platforms/iOS/Info.plist` + the csproj Release group is written but
   unverified.
2. Sign the Android `.aab` with the real upload keystore (values passed at
   build time, not committed) and confirm it uploads to Play Console.
3. Mark the Phase 11 milestone complete in
   `docs/roadmap/CONSOLIDATED_VIRTUAL_ADVOCATE_PI_ROADMAP.md`.

## Then

Start Phase 12 - GARP M 2026 change integration.

## Known gaps (tracked, not blockers)

- Firebase ID token has no silent-refresh (~1 hr expiry); a long session ends in
  the error state and needs re-sign-in.
- Evidence preparation-checklist tick state is per-device local only.
