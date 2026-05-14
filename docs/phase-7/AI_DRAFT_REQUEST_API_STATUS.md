# AI Draft Request API Status

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Build AI draft request API

## Status

Completed.

## Endpoint created

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts/request.

## Backend file created

backend/src/VirtualAdvocatePI.Api/Features/Ai/AiDraftRequestEndpoints.cs.

## Runtime files copied

backend/src/VirtualAdvocatePI.Api/KnowledgeBase/prompt-templates.

## Request contract

knowledge-base/prompt-templates/AI_DRAFT_REQUEST_API_CONTRACT.md.

## Behaviour

The endpoint gathers active workspace data.

The endpoint gathers selected active condition data where supplied.

The endpoint gathers active accepted-condition history, question responses, evidence items and evidence gaps.

The endpoint retrieves approved active source chunks.

The endpoint loads shared safety guardrails and task-specific prompt templates.

The endpoint returns a prompt package for future AI draft generation.

The endpoint records AI_DRAFT_REQUESTED.

## Safety boundary

This task builds the draft request API only.

It does not enable live AI generation.

It does not create final AI draft text.

It does not provide legal advice.

It does not provide medical advice.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not guarantee claim outcomes.

## Next task

Generate veteran statement and worsening summary drafts.
