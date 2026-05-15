# Clinical Question Generation Workflow Status

## App

Virtual Advocate PI

## Phase

Phase 9 - Doctor guidance pack

## Task

Build clinical question generation workflow

## Status

Completed.

## Frontend files created

web/src/components/DoctorGuidanceQuestionPanel.tsx.

web/src/app/claim-workspaces/[workspaceId]/doctor-guidance/page.tsx.

## Frontend files updated

web/src/components/WorkspaceToolNavigationPanel.tsx.

## Workflow behaviour

The doctor guidance page loads active workspace conditions.

The user can select a condition.

The user can generate doctor appointment questions.

The user can generate evidence gap discussion points.

The user can generate a doctor request letter.

The user can add appointment focus and extra context.

The workflow uses existing AI draft generation endpoints.

The workflow lists saved doctor guidance drafts for the selected condition.

The user can copy generated doctor guidance text.

The user can edit and save reviewed text.

The user can approve doctor guidance for later pack inclusion.

The user can reject doctor guidance material.

## Supported draft types

DOCTOR_QUESTIONS.

EVIDENCE_GAP_SUMMARY.

DOCTOR_REQUEST_LETTER.

## Doctor-specific safety boundary

The workflow is preparation support only.

It does not provide legal advice.

It does not provide medical advice.

It does not diagnose conditions.

It does not tell a doctor what opinion to provide.

It does not pressure a doctor to support a claim.

It does not ask a doctor to make a DVA decision.

It does not calculate impairment points.

It does not estimate compensation.

It does not guarantee claim outcomes.

It does not submit anything to DVA.

## Next task

Add doctor-specific disclaimer and review checklist.
