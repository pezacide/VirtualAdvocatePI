# Approved Source Registry Schema Status

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Create approved source registry schema

## Status

Completed.

## Backend entity

AiSourceRegistryEntry.

## Database table

ai_source_registry_entries.

## Migration

20260514215302_AddAiSourceRegistryEntries.

## Purpose

This table stores the approved source registry used by the AI/RAG workflow.

Only active and approved source registry entries should be used by future retrieval, prompt-building and AI draft generation workflows.

## Key fields

SourceKey.

Title.

Category.

SourceType.

Jurisdiction.

SourceVersion.

SourceDate.

CitationLabel.

SourceUrl.

StoragePath.

ContentHash.

ApprovalStatus.

IsActive.

ApprovedAt.

ApprovedBy.

ReviewNotes.

Status.

CreatedAt.

UpdatedAt.

## Safety boundary

The schema supports approved-source-only retrieval.

It does not enable AI generation by itself.

It does not use unapproved source material.

It does not provide legal advice, medical advice, DVA decisions, impairment estimates, compensation estimates or claim outcome guarantees.

## Next task

Create approved source registry and knowledge base structure.
