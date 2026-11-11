# Phase 12 Functional and Load Evidence Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 12 - Functional and Load Evidence tools

## Purpose

Confirm the functional impact (good day / bad day) tools, load exposure tools,
load reference library, admin seed, and the evidence annex in the Claim Starter
Pack and Doctor Guidance Pack all work end to end against the dev backend.

## Legend

- `[x]` verified automatically (backend build and tests).
- `[ ]` still to be done by the tester against the running web app and a real
  dev backend, database and document storage bucket.

## Pre-reqs

[x] Backend builds with 0 errors.
[x] Backend test suite passes (39 tests, including the annex and seed
    authorisation tests).
[ ] Web app builds and runs (`npm run dev`) and signs in with a test account.
[ ] Dev database has the Phase 12 migration `AddFunctionalLoadEvidence`
    applied.
[ ] Document storage bucket is configured (needed to generate packs).
[ ] Signed in as a non-admin test user, plus one admin user for the admin
    section.

## Test data to create first

[ ] A claim workspace with at least two active conditions.
[ ] Used by all sections below; archive nothing until the "Archive" checks.

## Functional impact (good day / bad day)

Route: `/claim-workspaces/{workspaceId}/functional-impact`

[ ] Page loads, shows the safety boundary text, and lists the conditions.
[ ] Create an entry for condition A: activity domain, good day text, bad day
    text, bad day frequency, aids/help. It appears in the list.
[ ] Create an entry with neither good day nor bad day text: shows
    "Describe a good day, a bad day, or both." and creates nothing.
[ ] Edit an entry; the change persists after a page reload.
[ ] Archive an entry; it disappears from the list and from the annex later.
[ ] An entry for condition A is not visible when condition B is selected.

## Functional and load question bank

Route: `/claim-workspaces/{workspaceId}/functional-load-questions`

[ ] All question groups load and show required markers.
[ ] Answer one question of each type present (text, long text, number, date,
    yes/no/unsure, single select); each shows saved.
[ ] Reload; all answers restore.
[ ] Safety boundary text is visible.

## Load exposure builder

Route: `/claim-workspaces/{workspaceId}/load-exposure`

[ ] Create one record of each type: lifting/carrying, stairs/ladders/rungs,
    kneeling/squatting, neck/shoulder carriage, heavy load carrying, hazard
    exposure.
[ ] Hazard exposure record requires/uses the hazard type; other types ignore it.
[ ] Weights accept decimals; negative or non-numeric weights are rejected.
[ ] Record can be linked to a condition and saved without one.
[ ] Edit and archive a record; changes persist after reload.
[ ] Records are listed grouped or filterable by type as designed.

## Load reference library

[ ] Lookup panel lists items (after the admin seed below has been run).
[ ] Searching or filtering narrows results.
[ ] Picking a reference item fills the typical weight in the builder; the
    weight is labelled as approximate.

## Workspace tool navigation

[ ] The workspace page links to Functional impact, Functional and load
    questions and Load exposure, and each link opens the right page.

## Claim Starter Pack annex

[ ] Generate a Claim Starter Pack for a workspace with test data above.
[ ] DOCX contains the "Functional and load evidence annex" section, placed
    before "Review and sign-off", with the safety note.
[ ] Good day / bad day entries appear grouped under the correct condition
    names with all recorded fields.
[ ] Load exposure records appear grouped by type with weights, frequency,
    service period and notes where recorded.
[ ] Archived entries and records are not in the annex.
[ ] PDF version contains the same annex content, wraps long lines, and is not
    cut off.
[ ] Generate a pack for a workspace with no functional/load data: the annex
    shows the "No ... recorded" messages and generation still succeeds.

## Doctor Guidance Pack annex

[ ] Generate a Doctor Guidance Pack with the same test data.
[ ] DOCX contains the annex before "Doctor guidance review checklist".
[ ] PDF contains the same annex content.
[ ] Annex wording restates recorded information only; no impairment points,
    capacity assessment or compensation estimate appears anywhere.

## Admin seed, templates and versions (admin user)

[ ] As a non-admin, `POST /api/v1/admin/functional-load-evidence/seed`
    returns 403. (Also covered by an automated test.)
[ ] As an admin, the seed returns 200 with 4 templates and 2 versions
    inserted.
[ ] Running the seed a second time inserts 0 entries.
[ ] `/admin/templates/questions` shows `FLE_QUESTION_BANK`,
    `FLE_GOOD_DAY_BAD_DAY` and `FLE_LOAD_EXPOSURE_BUILDER` as DRAFT.
[ ] `/admin/templates/documents` shows `FLE_EVIDENCE_ANNEX` as DRAFT.
[ ] `/admin/prompts-disclaimers` shows `FLE_DISCLAIMER_V1` and
    `FLE_PROMPT_GUARDRAIL_V1` as DRAFT.
[ ] Approving an entry without an approver name is rejected.
[ ] Load reference seed (`/api/v1/admin/reference/load-reference-items/seed`)
    works for an admin and is idempotent.

## Safety and privacy

[ ] No page or document describes the tools as calculating impairment,
    capacity, compensation or claim outcome.
[ ] A second test user cannot read or change the first user's functional
    impact entries or load records (expect 404 or 403).
[ ] Unauthenticated requests to the new endpoints return 401.

## Known gaps (not covered by Phase 12 so far)

- No mobile screens for these tools.
- AI draft generation does not yet use functional impact or load records.
- The admin seed has no UI button yet; it is called as an API endpoint.
- The roadmap's "register uploaded functional/load evidence templates" task is
  not yet defined against the ProjectLibre plan.

## Result

Date tested: ____________________

Tester: ____________________

Overall result (pass / fail): ____________________

Notes: ____________________________
