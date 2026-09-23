# Phase 11 Closeout Status

## App

Virtual Advocate PI

## Phase

Phase 11 - Android and iOS app MVP

## Status

Code complete. Milestone pending the tester's on-device pass (Android and iOS)
and the iOS build, which cannot be produced on this Windows machine.

## Completed tasks

Create MAUI app shell and shared services.

Configure mobile API environment settings.

Integrate Firebase Authentication.

Build authenticated API client and token flow.

Build dashboard and claim workspace screens.

Build condition intake and question engine screens.

Build GARP M question engine and structured summary screens.

Build evidence checklist and upload flow.

Build AI draft review and document download screens.

Add mobile loading, error and empty states.

Add Android app icon, name and basic release config.

Add iOS app icon, name and basic release config.

Add Phase 11 mobile smoke test checklist.

## Evidence screen

Available from the workspace Detail screen ("Evidence").

Static preparation checklist ported from the web app's EvidenceChecklistShell
(5 groups); tick state is stored per workspace in local Preferences only.

Per-condition evidence items: list, add by metadata, change is via re-add
(status defaults to LISTED_NOT_UPLOADED), remove behind a confirm dialog.

File upload: MAUI FilePicker -> POST evidence-upload-url -> HTTP PUT straight to
the Cloud Storage signed URL -> POST mark-uploaded. Uses a dedicated transfer
HttpClient with no API base address and no bearer token.

Open uploaded file: POST download-url -> open the signed GET URL in the browser.

Evidence gaps: read-only list with a "Recalculate" action (POST recalculate,
then re-read the full gap list).

## AI draft review screen

Available from the workspace Detail screen ("AI drafts").

Per-condition draft list; selecting a draft shows the original AI text
(read-only), an editable "Your version", the source references, and a safety
banner.

Actions via PATCH ai-drafts/{id}: save edits (USER_EDITED), save and approve
(APPROVED), reject (REJECTED), send back to review (USER_REVIEW_REQUIRED), and
archive (DELETE) behind a confirm dialog.

No on-device generation - drafts are generated on the web app.

## Generated documents screen

Available from the workspace Detail screen ("Generated documents").

Generate Claim Starter Pack and Doctor Guidance Pack; the response counts
(conditions, evidence items, gaps, included/excluded drafts) and the
reviewed-only rule text are shown.

Download DOCX or PDF via POST download-url and opening the signed GET URL. PDF
actions are hidden when no PDF path exists.

## Loading, error and empty states

The new screens follow the existing triad: ActivityIndicator on first load, a
red error card with "Try again", and a plain-English empty state.

Fixed a pre-existing gap: the GARP M question engine and structured summary
screens set HasError on a conditions-load failure but only rendered the error
inside the already-hidden HasConditions section. Both now show a top-level error
card with "Try again".

Empty-title validation on the new-workspace form already exists in
NewClaimWorkspaceViewModel.CreateAsync; it is listed for an explicit on-device
check.

## App identity and release config

ApplicationTitle is "Virtual Advocate PI". ApplicationId is
au.com.virtualadvocatepi.mobile (was the com.companyname template placeholder).

The .NET template icon/splash were replaced with a flat brand mark (cyan shield
and check on #0B3B45); icon and splash colours match.

Android Release: aab package format, SdkOnly linking, no debug symbols; AOT is
opt-in via /p:EnableMobileAot=true (needs the Android NDK). Signing keystore
values are passed at build time and never committed.

iOS Release (built on macOS): ios-arm64, SdkOnly link, Entitlements.plist;
codesign identity and provisioning profile passed at build time. Info.plist
carries the display name, bundle id/version from MSBuild props, and
ITSAppUsesNonExemptEncryption = false.

AndroidManifest sets android:usesCleartextTraffic="false".

## Build and verification

Android Debug build: passed (0 warnings / 0 errors).

Android Release build: passed; produced au.com.virtualadvocatepi.mobile-Signed.aab.

Windows build: passed (0 warnings / 0 errors).

Release APK installed and launched on the Pixel_10_Pro_XL emulator with no
startup crash; the app opened to the Sign in screen with the new branding.

iOS build: not attempted - requires macOS.

Full authenticated end-to-end flows (upload, AI draft review, pack generation
and download) were not exercised here: they need a valid Firebase account and
device interaction. They are the open items in
docs/phase-11/PHASE_11_MOBILE_SMOKE_TEST_CHECKLIST.md.

No backend changes were needed; every endpoint the mobile app calls already
existed and is exercised by the web client.

## Safety boundary

Every new screen carries a preparation-support-only boundary statement.

The app does not calculate GARP M impairment points, estimate compensation,
provide legal or medical advice, make or imply DVA decisions, or guarantee a
claim outcome.

AI-assisted draft text is shown as requiring user review before use.

No backend secrets, service account keys or database connection strings are in
the mobile app.

## Known gaps (tracked)

Firebase ID token silent-refresh is still absent (~1 hr expiry). Tracked for a
later phase.

Evidence preparation-checklist tick state is per-device local only.

## Recommended next phase

Phase 12 - Functional and Load Evidence tools (per ProjectLibre).
Phase 13 is GARP M 2026 change integration.
