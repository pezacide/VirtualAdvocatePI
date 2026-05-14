# AI Draft Review, Copy and Save Workflow Status

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Add AI draft review, copy and save workflow

## Status

Completed.

## Frontend files updated

web/src/lib/api/aiDrafts.ts.

web/src/components/AiDraftReviewPanel.tsx.

## Existing backend support confirmed

The backend already supports listing workspace and condition AI drafts.

The backend already supports updating draft text and review status.

The backend already supports approving and rejecting drafts through ReviewStatus.

The backend already supports archiving AI drafts.

The backend deterministic generation endpoint creates reviewable drafts.

## Workflow added

Generate reviewable AI preparation drafts.

View saved active drafts.

Copy draft text.

Edit reviewed draft text.

Save draft review.

Approve draft.

Reject draft.

Archive draft.

Show source references.

## Audit events

AI_DRAFT_CREATED.

AI_DRAFT_UPDATED.

AI_DRAFT_ARCHIVED.

## Safety boundary

Drafts are preparation support only.

Drafts require user review before use.

This workflow does not submit anything to DVA.

It does not provide legal advice.

It does not provide medical advice.

It does not diagnose conditions.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not guarantee claim outcomes.

## Next task

Add AI/RAG audit logging.
