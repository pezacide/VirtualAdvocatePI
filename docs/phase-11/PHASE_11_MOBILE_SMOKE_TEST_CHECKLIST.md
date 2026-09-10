# Phase 11 Mobile Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 11 - Android and iOS app MVP (full app)

## Purpose

Confirm every Phase 11 mobile screen works end to end against the real dev
backend and Firebase authentication, on a real device or emulator, before
Phase 11 is closed out.

## Legend

- `[x]` verified in this repo's environment (Windows + `Pixel_10_Pro_XL`
  Android emulator, Release build).
- `[ ]` still to be done by the tester (needs a real Firebase account, a real
  device, or a Mac).

## Pre-reqs

[x] Android Debug build passes (`-f net10.0-android`), 0 warnings / 0 errors.
[x] Android Release build produces a signed `.aab` and `.apk`, 0 warnings /
    0 errors.
[x] Windows build passes (`-f net10.0-windows10.0.19041.0`), 0 warnings /
    0 errors.
[ ] iOS build passes on a Mac (`-f net10.0-ios`) - cannot be built on Windows.
[ ] Backend dev API reachable (`/health` returns 200) from the test device.
[ ] Signed in with a real Firebase test account.

## App launch and branding

[x] Release APK installs and launches without a startup crash (confirms the new
    Evidence / AI draft / generated-document DI registrations resolve).
[x] App name shows as "Virtual Advocate PI"; launcher icon is the brand shield
    mark, not the .NET template icon.
[x] Sign in screen renders with its "Preparation support only" disclaimer.
[ ] Splash screen shows the brand mark on the dark ground.

## Sign in, disclaimer, dashboard (regression)

[ ] Sign in with a valid email/password reaches the Disclaimer gate on first
    sign-in, and the Dashboard after acceptance.
[ ] Second sign-in auto-skips the Disclaimer gate and lands on the Dashboard.
[ ] Dashboard shows a loading indicator, then the workspace list or the empty
    state.
[ ] "New claim workspace" creates a workspace and opens its Detail screen; one
    back tap returns to the Dashboard; the new workspace appears in the list.
[ ] New workspace form: clearing the title and tapping Create shows
    "Enter a workspace title." and creates nothing. (Validation is in
    `NewClaimWorkspaceViewModel.CreateAsync`; confirm it on-device.)

## Workspace detail - tool navigation

[ ] Detail screen shows Conditions, GARP M questions, Structured summary,
    Evidence, AI drafts and Generated documents buttons (no "on the way"
    placeholder).
[ ] Each button opens its screen with the workspace context; back returns to
    Detail.

## Conditions / GARP M (regression)

[ ] Add a condition; it appears in the list.
[ ] GARP M question engine: a conditions-load failure now shows a red error card
    with a working "Try again" (new in this phase).
[ ] Structured summary: same conditions-load error + retry behaviour; summary
    still builds and "Copy summary" works.

## Evidence screen

[ ] Preparation checklist renders all 5 groups; ticking an item persists across
    leaving and re-opening the screen (per-workspace local state).
[ ] With no conditions: the "Add a condition first" card shows.
[ ] Condition picker lists the workspace's conditions.
[ ] "Add to evidence list": pick a type, optional provider/date/notes, submit;
    the item appears with status "Listed, not uploaded".
[ ] "Pick a file and upload": choose a small PDF (< 1 MB); progress indicator
    shows; on success the item's status becomes "Uploaded" and the file name is
    shown.
[ ] Upload rejects a file over 25 MB with a readable message (backend 400).
[ ] "Open file" on an uploaded item opens the signed URL in the browser /
    viewer.
[ ] "Remove" prompts for confirmation, then the item disappears from the list.
[ ] Evidence gaps list renders; "Recalculate evidence gaps" updates it.
[ ] Killing the Firebase token mid-session shows the error state; signing back
    in and "Try again" recovers.

## AI drafts screen

[ ] With existing drafts for a condition: the draft list shows type + review
    status per draft.
[ ] Selecting a draft shows the original AI text (read-only), an editable "Your
    version", the source references block, and the safety banner.
[ ] "Save edits" persists the edited text and sets status to "Edited by you".
[ ] "Save and mark approved" sets status to "Approved".
[ ] "Reject draft" sets status to "Rejected"; "Send back to needs-review" sets
    "Needs review".
[ ] "Archive draft" confirms, then removes the draft from the list.
[ ] Condition with no drafts shows the empty state, not an error.

## Generated documents screen

[ ] "Generate Claim Starter Pack" succeeds and shows the returned counts
    (conditions / evidence items / gaps / included approved drafts / excluded
    unapproved drafts) and the reviewed-only rule text.
[ ] "Generate Doctor Guidance Pack" succeeds and shows its counts.
[ ] The generated document appears in the list with type, status, generated
    time and template version.
[ ] "Download DOCX" opens the signed URL; "Download PDF" is only shown when a
    PDF path exists, and the "PDF not available yet" note shows otherwise.
[ ] Empty state shows for a workspace with no generated documents.

## Loading / error / empty states (all screens)

[ ] Every screen shows a loading indicator on first load, then content or a
    plain-English empty state.
[ ] Every screen's error state has a working "Try again".

## Release config

[x] Android: `ApplicationId` is `au.com.virtualadvocatepi.mobile`;
    Release builds an `.aab`; `usesCleartextTraffic="false"` in the manifest.
[ ] Android: signed with the real upload keystore (keystore values passed at
    build time, not committed).
[ ] iOS: `CFBundleDisplayName` = "Virtual Advocate PI", bundle id matches,
    `ITSAppUsesNonExemptEncryption` = false; archived with a distribution
    profile on a Mac.

## Device testing

[ ] Android: full checklist run on at least one physical Android device.
[ ] iOS: full checklist run on at least one physical iOS device.

## Known gaps (tracked, not blockers for this checklist)

- Firebase ID tokens have no silent-refresh (~1 hr expiry); a long session
  will hit the error state and require re-sign-in. Tracked for a later phase.
- Evidence preparation-checklist tick state is per-device local only (matches
  the web app, which does not persist it).
- On-device AI draft generation is intentionally not included; drafts are
  generated on the web app and only reviewed on mobile.

## Close-out

[ ] All unchecked items above completed by the tester.
[ ] Milestone: Phase 11 complete.
