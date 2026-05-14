# Ingestion and Metadata Rules

## Status

Draft placeholder.

## Core rule

Do not ingest unknown, unapproved or unreviewed source material into AI/RAG retrieval.

## Required metadata

SourceKey.

Title.

Category.

SourceType.

Jurisdiction.

SourceVersion.

SourceDate where available.

CitationLabel.

SourceUrl or StoragePath.

ContentHash.

ApprovalStatus.

IsActive.

ReviewNotes.

## Approval rule

Retrieval must only use entries where ApprovalStatus is APPROVED, IsActive is true, and Status is ACTIVE.

## Next task dependency

Create source category taxonomy.
