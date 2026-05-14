# Phase 6 Smoke Test and Closure Checklist

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Status

Ready for smoke testing.

## Build checks

- [ ] Backend build passes with dotnet build.
- [ ] Web build passes with npm run build.

## Required local services

- [ ] Backend API is running.
- [ ] Web app is running.
- [ ] User can sign in.
- [ ] A claim workspace exists.
- [ ] At least one condition exists in the workspace.

## Smoke test pages

- [ ] Workspace dashboard opens.
- [ ] Workspace tool navigation opens.
- [ ] Condition intake page opens.
- [ ] Accepted-condition history page opens.
- [ ] GARP M questions page opens.
- [ ] Evidence metadata page opens.
- [ ] Evidence upload page opens.
- [ ] Evidence gaps page opens.
- [ ] Evidence audit trail page opens.

## Evidence upload validation

- [ ] Evidence upload page shows supported file wording.
- [ ] File picker prefers PDF, image, Word, text and RTF files.
- [ ] Missing file shows a clear error.
- [ ] Unsupported file type shows a clear error when selected or uploaded.
- [ ] Oversized file is blocked before upload where possible.
- [ ] Backend upload validation rejects unsupported or oversized upload URL requests.
- [ ] Small supported file uploads successfully.
- [ ] Uploaded file appears in the evidence list.
- [ ] Open file action works for uploaded evidence.

## Evidence metadata

- [ ] Metadata page loads conditions.
- [ ] Metadata page shows evidence list summary.
- [ ] Evidence type labels are readable.
- [ ] Evidence category labels are readable.
- [ ] Provider/source quick tags work.
- [ ] Evidence status buttons work.
- [ ] Status changes reload after refresh.

## Evidence gaps

- [ ] Evidence gaps page loads conditions.
- [ ] Recalculate evidence gaps works.
- [ ] Gap type labels are readable.
- [ ] Severity labels are readable.
- [ ] Gap status labels are readable.
- [ ] Gap status can be changed to Open, In progress, Resolved and Not applicable.
- [ ] Gap dashboard appears.
- [ ] Reminder prompts appear.
- [ ] High-priority gap focus appears when high-priority gaps exist.

## GARP M connection

- [ ] GARP M questions page saves answers.
- [ ] Medication answers can influence evidence gap prompts.
- [ ] Functional impact answers can influence evidence gap prompts.
- [ ] Previous compensation or accepted-history answers can influence evidence gap prompts.
- [ ] Worsening answers can influence evidence gap prompts.
- [ ] Evidence gap or appointment preparation answers can influence follow-up prompts.

## Audit trail

- [ ] Evidence audit trail link appears in workspace tools.
- [ ] Audit trail page opens.
- [ ] Audit event summary cards appear.
- [ ] Evidence-only filter works.
- [ ] Refresh audit trail works.
- [ ] Evidence upload, metadata, gap or generated document events appear when present.

## Safety wording

- [ ] Evidence upload page includes preparation-only wording.
- [ ] Evidence metadata page includes preparation-only wording.
- [ ] Evidence gap page includes preparation-only wording.
- [ ] Evidence audit trail page includes preparation-only wording.
- [ ] No page claims to submit evidence to DVA.
- [ ] No page guarantees claim success.
- [ ] No page provides legal or medical decision advice.

## Phase 6 closure decision

- [ ] Builds pass.
- [ ] Core pages open.
- [ ] Evidence workflow works end to end.
- [ ] Known issues are documented.
- [ ] Phase 6 can be closed.

## Known issues

Record any issues found during smoke testing here.

## Closure note

Phase 6 is complete when the checklist above has been smoke-tested and any blocking issues have been fixed or documented.
