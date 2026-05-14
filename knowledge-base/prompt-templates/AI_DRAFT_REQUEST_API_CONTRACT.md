# AI Draft Request API Contract

## Endpoint

POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts/request

## Purpose

This endpoint creates a safe AI draft request package.

It gathers active workspace data, approved source chunks, citation/source references and prompt templates.

It does not call an AI model yet.

It does not create a final AI draft yet.

## Request fields

ConditionId.

DraftTaskType.

Query.

MaxSources.

UserInstruction.

## Supported draft task types

VETERAN_STATEMENT.

WORSENING_SUMMARY.

EVIDENCE_GAP_SUMMARY.

DOCTOR_QUESTIONS.

DOCTOR_REQUEST_LETTER.

CLAIM_PACK_COVER_NOTE.

## Response includes

Workspace ID.

Condition ID.

Draft task type.

AI draft type.

Prompt version.

Query.

User instruction.

Source references.

Workspace data summary counts.

Prompt package.

Safety flags.

## Audit event

AI_DRAFT_REQUESTED.

## Safety boundary

The endpoint prepares a prompt package only.

It does not generate AI draft text.

It does not provide legal advice.

It does not provide medical advice.

It does not diagnose conditions.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not guarantee claim outcomes.

It does not submit anything to DVA.
