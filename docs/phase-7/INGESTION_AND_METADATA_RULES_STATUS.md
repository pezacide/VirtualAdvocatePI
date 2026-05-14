# Ingestion and Metadata Rules Status

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Create ingestion and metadata rules

## Status

Completed.

## Files created or updated

knowledge-base/ingestion/INGESTION_AND_METADATA_RULES.md.

knowledge-base/ingestion/ingestion-metadata-rules.json.

knowledge-base/ingestion/Test-KnowledgeBaseSeedContent.ps1.

## Validation

The validation script checks that seed chunks have required metadata, approved source keys, valid taxonomy categories, citation labels, safety notes and active approved status.

## Eligibility rule

Future retrieval should only use chunks and sources where approvalStatus is APPROVED, isActive is true and status is ACTIVE.

## Safety boundary

This task creates ingestion and metadata controls only.

It does not enable live AI generation.

It does not provide legal advice.

It does not provide medical advice.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not guarantee claim outcomes.

## Next task

Build RAG retrieval API.
