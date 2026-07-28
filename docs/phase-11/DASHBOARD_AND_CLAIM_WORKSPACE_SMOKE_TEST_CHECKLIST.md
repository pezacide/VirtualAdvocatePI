# Dashboard and Claim Workspace Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 11 - Android and iOS app MVP (Dashboard, disclaimer, and claim workspace screens)

## Purpose

This checklist confirms that the mobile Dashboard, mandatory Disclaimer gate, New Claim
Workspace and Claim Workspace Detail screens work end to end against the real dev backend
and Firebase authentication, before starting the next Phase 11 screens.

## Pre-reqs

[x] Backend dev API is reachable (`/health` returns 200).
[x] Unauthenticated `/api/v1/claim-workspaces` returns 401.
[x] Backend test suite passes (22/22, including disclaimer-acceptance endpoints).
[x] Mobile app builds clean for Windows and Android (0 warnings, 0 errors).
[x] Signed in with a real Firebase test account, run on the Android emulator.

## Sign-in and navigation

[x] Sign in with a valid email/password.
[x] Confirm sign-in redirects to the Disclaimer gate on first sign-in.
[x] Confirm accepting the disclaimer redirects to the Dashboard (not the old Home diagnostic
    screen).
[x] Sign out and sign back in with the same account: confirm the Disclaimer gate is
    auto-skipped (already accepted) and lands straight on Dashboard.
[x] Confirmed the auto-skip persists even after a full app reinstall (acceptance is recorded
    server-side, not device-local).
[x] Confirm the Dashboard shows a loading indicator briefly, then either the workspace list
    or the empty state.

## Dashboard

[x] Confirm existing workspaces are listed with title, scenario and status.
[x] Confirm the empty state message appears if the account has no workspaces yet.
[x] Tap "New claim workspace" and confirm it opens the New Workspace screen.
[x] Tap an existing workspace card and confirm it opens that workspace's Detail screen.
[x] Confirm a visible "Sign out" button is present in the Dashboard header.

## New claim workspace

[x] Confirm a claim-scenario card is selected by default ("Not sure yet").
[x] Tap a different scenario card and confirm the selected card visually highlights
    (cyan border/background) and the previous selection un-highlights; confirmed the
    highlight persists through scrolling.
[ ] Clear the workspace title field and tap Create; confirm a validation message appears
    and no workspace is created. (Not automatable in this pass; do this by hand.)
[x] Enter a title, keep a scenario selected, tap Create.
[x] Confirm it navigates to the new workspace's Detail screen (not back to Dashboard).
[x] Tap Cancel on a fresh New Workspace screen and confirm it returns to the Dashboard
    without creating anything.

## Claim workspace detail

[x] Confirm the workspace title, status, scenario, framework, generated-pack status and
    workspace ID are all displayed correctly for a freshly created workspace.
[x] Use the platform back button/arrow and confirm it returns to the Dashboard in one tap
    (previously took two taps due to a back-stack depth bug — fixed and re-verified with a
    second freshly created workspace).
[x] Confirm the newly created workspace now appears in the Dashboard list (previously failed
    due to a Dashboard reload bug — fixed and re-verified).
[x] Re-open the same workspace from the Dashboard and confirm the same details load again.

## Error handling

[x] Confirmed the Dashboard error state (title, message, "Try again" button) renders
    correctly — triggered naturally by an expired Firebase ID token mid-session rather than
    by disabling network, but it is the same code path.
[x] Confirm "Try again" reloads successfully once signed in again with a fresh token.

## Sign-out

[x] Use the Dashboard header's "Sign out" button and confirm it returns to the Login screen.
[x] Confirm signing back in returns to the Dashboard (via the Disclaimer auto-skip).

## Build checks

[x] Backend build passes.
[x] Backend test suite passes (22/22).
[x] Mobile Windows build passes.
[x] Mobile Android build passes.
[x] Mobile app run and exercised end to end on the Android emulator (this checklist).

## Bugs found during this pass (all fixed and re-verified)

- Dashboard only loaded its workspace list once, in the ViewModel constructor; Shell reuses
  the same page/ViewModel instance for a ShellContent route, so newly created workspaces
  never appeared until the app was restarted. Fixed by moving the load into the page's
  `OnAppearing`.
- No way to reach a sign-out control from the normal post-login flow (the only sign-out
  button lived on the orphaned diagnostic Home screen, which isn't reachable without a
  flyout/tab bar). Fixed by adding a Sign out button to the Dashboard header.
- Creating a workspace pushed the Detail screen on top of the New Workspace form
  (Dashboard -> NewWorkspace -> Detail), so one back-tap from Detail returned to the stale
  form instead of Dashboard. Fixed by resetting the navigation stack to Dashboard -> Detail
  on successful creation.
- The backend `Dockerfile`'s `COPY` paths assumed the wrong build context, blocking dev
  Cloud Run redeploys entirely. Fixed and redeployed successfully.

## Known gaps (expected, not a failure)

- The Detail screen shows a placeholder note that condition/evidence/document tools are
  still to come — this is correct for this stage of Phase 11.
- Empty-title validation on the New Workspace form was not exercised via automation in this
  pass (tap-focus timing issues); worth a quick manual check.
- Firebase ID tokens have no silent-refresh flow (~1hr expiry) — a known, already-tracked
  gap, encountered naturally during this test session.

## Close-out

[x] Dashboard screen = confirmed working.
[x] Disclaimer gate screen = confirmed working (mandatory + auto-skip).
[x] New claim workspace screen = confirmed working.
[x] Claim workspace detail screen = confirmed working.
[x] Error state handling = confirmed working.
[x] Ready to continue to condition intake and question engine screens.
