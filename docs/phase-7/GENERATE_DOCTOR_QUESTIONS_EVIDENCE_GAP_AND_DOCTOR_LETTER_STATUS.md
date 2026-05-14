# Generate Doctor Questions, Evidence Gap Summary and Doctor Letter Status

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Generate doctor questions, evidence gap summary and doctor letter

## Status

Completed.

## Endpoint extended

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts/generate.

## Draft types added

DOCTOR_QUESTIONS.

EVIDENCE_GAP_SUMMARY.

DOCTOR_REQUEST_LETTER.

## Behaviour

The endpoint requires a selected active condition.

The endpoint gathers active workspace data for the selected condition.

The endpoint excludes archived condition history, question responses, evidence items and evidence gaps.

The endpoint retrieves approved active knowledge-base source chunks.

The endpoint creates reviewable draft records in AiDrafts.

The endpoint stores source references as JSON.

The endpoint sets ReviewStatus to USER_REVIEW_REQUIRED.

The endpoint records AI_DRAFT_CREATED.

## Build repair

The switch logic had been patched before the three new builder methods were inserted.

The missing builder methods were added for doctor questions, evidence gap summary and doctor request letter.

Backend build now passes.

## Contract

knowledge-base/prompt-templates/AI_DRAFT_GENERATION_EXTENDED_API_CONTRACT.md.

## Safety boundary

This endpoint creates reviewable preparation drafts only.

The current version does not call a live AI model.

It does not provide legal advice.

It does not provide medical advice.

It does not diagnose conditions.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not guarantee claim outcomes.

It does not submit anything to DVA.

## Next task

Add AI draft review, copy and save workflow.
