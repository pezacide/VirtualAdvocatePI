# AI Draft Generation Extended API Contract

## Endpoint

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts/generate

## Purpose

This endpoint creates reviewable preparation drafts for supported Phase 7 draft types.

## Supported draft types after this task

VETERAN_STATEMENT.

WORSENING_SUMMARY.

DOCTOR_QUESTIONS.

EVIDENCE_GAP_SUMMARY.

DOCTOR_REQUEST_LETTER.

## Saved draft state

ReviewStatus is USER_REVIEW_REQUIRED.

Status is ACTIVE.

PromptVersion is deterministic-rag-draft-v1.

SourceReferences stores citation/source metadata as JSON.

## Safety boundary

The current version creates deterministic reviewable drafts only.

It does not call a live AI model.

It does not provide legal advice.

It does not provide medical advice.

It does not diagnose conditions.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not guarantee claim outcomes.

It does not submit anything to DVA.
