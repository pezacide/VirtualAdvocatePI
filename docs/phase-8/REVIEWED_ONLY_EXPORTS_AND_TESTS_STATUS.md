# Reviewed-Only Exports and Test Exports Status

## App

Virtual Advocate PI

## Phase

Phase 8 - Claim Starter Pack document generation

## Task

Enforce reviewed-only content and test exports

## Status

Completed.

## Backend enforcement

The Claim Starter Pack generation endpoint includes approved AI drafts only.

The endpoint counts active unapproved AI drafts that were excluded.

The endpoint returns excludedUnapprovedAiDraftCount.

The endpoint returns a reviewedOnlyRule message.

The DOCX export includes reviewed-only inclusion wording.

The PDF export includes reviewed-only inclusion wording.

## Audit event added

CLAIM_STARTER_PACK_REVIEWED_ONLY_ENFORCED.

## Checklist created

docs/phase-8/CLAIM_STARTER_PACK_EXPORT_SMOKE_TEST_CHECKLIST.md.

## Safety boundary

Generated exports are preparation support only.

They do not submit anything to DVA.

They do not provide legal advice.

They do not provide medical advice.

They do not make DVA decisions.

They do not calculate impairment points.

They do not estimate compensation.

They do not guarantee claim outcomes.

## Next task

Milestone: Phase 8 complete.
