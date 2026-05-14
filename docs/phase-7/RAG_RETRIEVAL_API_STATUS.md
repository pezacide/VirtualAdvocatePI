# RAG Retrieval API Status

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Build RAG retrieval API

## Status

Completed.

## Endpoint created

POST /api/v1/claim-workspaces/{workspaceId}/ai-rag/retrieve.

## Request fields

ConditionId.

DraftTaskType.

Query.

MaxResults.

## Response includes

Workspace ID.

Condition ID where supplied.

Draft task type.

Query.

Returned chunk count.

Source references.

Retrieved chunks.

Safety flags.

## Runtime knowledge base

The API project now includes a runtime copy of knowledge-base-chunks.seed.jsonl.

The API project file publishes the KnowledgeBase folder to output.

## Retrieval eligibility rule

Chunks are eligible only where ApprovalStatus is APPROVED, IsActive is true and Status is ACTIVE.

Chunks must have ChunkKey, SourceKey, CitationLabel and Content.

## Audit event

AI_RAG_RETRIEVAL_REQUESTED.

## Safety boundary

The endpoint performs retrieval only.

It does not generate AI drafts.

It does not provide legal advice.

It does not provide medical advice.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not guarantee claim outcomes.

## Next task

Build retrieval response with citations/source references.
