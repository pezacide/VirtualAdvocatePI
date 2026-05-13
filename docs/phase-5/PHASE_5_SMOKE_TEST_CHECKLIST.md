# Phase 5 Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 5 - GARP M-aware question engine

## Purpose

Use this checklist to confirm the Phase 5 GARP M-aware question engine, summary screen and related web usability fixes still work.

## Safety boundary

The Phase 5 tools are preparation support only.

They do not calculate GARP M impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## 1. Backend smoke test

Run:

cd C:\Projects\VirtualAdvocatePI
.\scripts\api\smoke-test-dev-api.ps1

Expected:

All smoke tests passed.

## 2. Backend build

Run:

cd C:\Projects\VirtualAdvocatePI\backend\src\VirtualAdvocatePI.Api
dotnet build

Expected:

Build succeeded.

## 3. Web build

Run:

cd C:\Projects\VirtualAdvocatePI\web
npm run build

Expected:

Compiled successfully.

## 4. Start web app

Run:

npm run dev

Open:

http://localhost:3000

## 5. Login page

Open:

http://localhost:3000/login

Expected:

Small mode buttons show Existing account and New account.
Main submit button shows Sign in when Existing account is selected.
Main submit button shows Create account when New account is selected.
Sign in works with a Firebase test account.

## 6. Dashboard workspace cards

Open:

http://localhost:3000/dashboard

Expected:

Claim workspace cards load.
Each workspace card shows condition names.
If a workspace has no conditions, it shows Conditions: No conditions added yet.

## 7. Condition intake diagnosis status

Open:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/conditions

Test each diagnosis status:

DIAGNOSED
SUSPECTED
UNSURE
NOT_DIAGNOSED

Expected:

All four diagnosis status values save without backend validation errors.

## 8. Condition intake date picker

On the condition intake page, test Date diagnosed.

Expected:

A visible Choose date button appears.
Clicking Choose date opens the browser date picker.
Selected date is saved with the condition.

## 9. GARP M question engine route

Open:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/garp-m-questions

Expected:

The page loads.
A condition can be selected.
The safety boundary is visible.
Question sections are visible.
Save and resume progress panel is visible.

## 10. GARP M question groups

Test these sections:

Diagnosis, symptoms and treatment
Stability and treatment response
Functional, lifestyle and work impact
Worsening and previous compensation
Evidence gaps and appointment preparation

Expected:

Each section renders questions.
Answers can be entered.
Save this section works without invalid question group errors.
Saved answers reload after refresh.

## 11. GARP M date picker controls

Test date questions in the GARP M question engine.

Expected:

Date questions show a visible Choose date button.
Clicking Choose date opens the browser date picker.
Selected dates save and reload.

## 12. Save and resume support

Open the GARP M question engine and answer several questions across two sections.

Expected:

Sections started count updates.
Required sections complete count updates.
Last saved time updates.
Section cards show answered count, saved count and required missing count.
Continue next incomplete section moves to the next incomplete section.

## 13. Validation and missing-answer prompts

Open Diagnosis, symptoms and treatment.

Test:

Leave a required question blank.
Enter a very short answer for a field with a minimum length rule.

Expected:

Required missing answers are visibly flagged.
Validation messages appear in plain English.
Validation issue counts appear on the section card and save panel.

## 14. Structured summary route

Open:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/garp-m-summary

Expected:

The summary page loads.
A condition can be selected.
Saved GARP M answers appear grouped by section.
Missing required answers are listed.
A copyable plain-English summary appears.

## 15. Structured summary copy feedback

Click Copy summary.

Expected:

The button changes to Copied.
A green copied message appears under the button.
The copied summary can be pasted into another place.

## 16. Workspace detail Phase 5 links

Open:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID

Expected:

A GARP M-aware preparation tools section appears.
The question engine card opens the GARP M questions route.
The summary card opens the GARP M summary route.

## 17. Remaining date picker controls

Test these pages:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/accepted-history
http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/evidence-metadata
http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/evidence-upload

Expected:

Previous decision date has a visible Choose date button.
Previous assessment date has a visible Choose date button.
Evidence metadata document date has a visible Choose date button.
Evidence upload document date has a visible Choose date button.
Saving still works.

## 18. Sign out

Use the header sign-out button.

Expected:

The user signs out.
Protected routes redirect to login.
Public routes still load.

## Pass criteria

Phase 5 smoke test passes when:

Backend smoke test passes.
Backend build succeeds.
Web build succeeds.
Login labels are clear.
Dashboard cards show condition names.
Condition intake diagnosis statuses match backend allowed values.
All date controls show visible Choose date buttons.
GARP M question engine saves and reloads answers.
Save and resume progress works.
Validation prompts appear.
Structured summary loads saved answers.
Copy summary gives visible feedback.
Workspace detail links open the Phase 5 tools.
Safety boundary wording remains visible.
