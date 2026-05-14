# AI Draft Generation API Contract

## Endpoint

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts/generate

## Purpose

This endpoint creates a reviewable preparation draft for selected supported draft types.

## Supported draft types

VETERAN_STATEMENT.

WORSENING_SUMMARY.

## Saved draft state

ReviewStatus is USER_REVIEW_REQUIRED.

Status is ACTIVE.

PromptVersion is deterministic-rag-draft-v1.

SourceReferences stores citation/source metadata as JSON.

## Safety boundary

The current version does not call a live AI model.

It does not provide legal advice, medical advice, DVA decisions, impairment estimates, compensation estimates or claim outcome guarantees.
