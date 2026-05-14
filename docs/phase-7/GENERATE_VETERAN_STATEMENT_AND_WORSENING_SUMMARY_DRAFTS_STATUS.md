# Generate Veteran Statement and Worsening Summary Drafts Status

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Generate veteran statement and worsening summary drafts

## Status

Completed.

## Endpoint created

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts/generate.

## Backend file created

backend/src/VirtualAdvocatePI.Api/Features/Ai/AiDraftGenerationEndpoints.cs.

## Supported draft types

VETERAN_STATEMENT.

WORSENING_SUMMARY.

## Behaviour

The endpoint requires a selected active condition.

The endpoint gathers active workspace data for the selected condition.

The endpoint excludes archived condition history, question responses, evidence items and evidence gaps.

The endpoint retrieves approved active knowledge-base source chunks.

The endpoint creates a reviewable draft record in AiDrafts.

The endpoint stores source references as JSON.

The endpoint sets ReviewStatus to USER_REVIEW_REQUIRED.

The endpoint records AI_DRAFT_CREATED.

## Build repair

The endpoint was adjusted to avoid directly referencing a SymptomsSummary property that does not exist on the current ClaimCondition model.

A helper now safely checks for common condition text properties and falls back to placeholder wording when no matching property exists.

## Contract

knowledge-base/prompt-templates/AI_DRAFT_GENERATION_API_CONTRACT.md.

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

Generate doctor questions, evidence gap summary and doctor letter.
