# Knowledge Base Seed Content

## Purpose

This folder contains machine-readable seed chunks for the Virtual Advocate PI Phase 7 AI/RAG knowledge base.

The seed chunks are designed for future ingestion and retrieval testing.

## File

knowledge-base-chunks.seed.jsonl.

## Chunk rules

Each line is one JSON object.

Each chunk has a sourceKey that maps back to the approved source registry.

Each chunk has a chunkKey, category, sourceType, citationLabel, retrievalUse, content and safetyNotes.

Only chunks with approvalStatus APPROVED, isActive true and status ACTIVE should be considered eligible for future retrieval.

## Safety boundary

These seed chunks do not enable AI generation.

They do not provide legal advice.

They do not provide medical advice.

They do not calculate impairment points.

They do not estimate compensation.

They do not make DVA decisions.

They do not guarantee claim outcomes.

## Current chunk groups

Safety guardrails.

Veteran statement template.

Worsening summary template.

Doctor questions template.

Evidence gap summary template.

Doctor request letter template.

Claim pack cover note template.

GARP M preparation context.

GARP M lifestyle and functional impact context.

DVA reform context.

Permanent impairment reform context.
