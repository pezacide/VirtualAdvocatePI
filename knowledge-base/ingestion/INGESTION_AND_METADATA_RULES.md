# Ingestion and Metadata Rules

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Purpose

This document defines the rules for adding source material to the Virtual Advocate PI AI/RAG knowledge base.

The ingestion workflow must protect the app from using unknown, unapproved, outdated, unsafe or poorly labelled source material.

## Core rule

Do not ingest unknown, unapproved or unreviewed source material into AI/RAG retrieval.

Future retrieval and prompt-building must only use source chunks where the linked source registry entry is approved, active and not archived.

## Approved-source eligibility

A source is eligible for retrieval only when all of the following are true.

ApprovalStatus is APPROVED.

IsActive is true.

Status is ACTIVE.

SourceKey is present and unique.

Category exists in the approved source category taxonomy.

SourceType is valid for that category.

CitationLabel is present.

StoragePath or SourceUrl is present.

ReviewNotes are present.

## Disallowed source states

Do not retrieve sources where ApprovalStatus is DRAFT.

Do not retrieve sources where ApprovalStatus is NEEDS_REVIEW.

Do not retrieve sources where ApprovalStatus is REJECTED.

Do not retrieve sources where IsActive is false.

Do not retrieve sources where Status is ARCHIVED.

Do not retrieve sources with missing SourceKey.

Do not retrieve sources with missing CitationLabel.

Do not retrieve sources that are not mapped to the taxonomy.

## Required source registry metadata

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

ApprovedAt where applicable.

ApprovedBy where applicable.

ReviewNotes.

Status.

CreatedAt.

UpdatedAt.

## Required chunk metadata

ChunkKey.

SourceKey.

Category.

SourceType.

CitationLabel.

ChunkTitle.

RetrievalUse.

Content.

SafetyNotes.

ApprovalStatus.

IsActive.

Status.

## Chunking rules

Chunks should be short enough for targeted retrieval.

Chunks should contain one main idea or one reusable template section.

Chunks must keep the SourceKey from the approved source registry.

Chunks must keep a CitationLabel.

Chunks must not mix unrelated source categories.

Chunks must not remove required safety context.

Chunks must not copy excessive source text from external references.

Chunks should summarise and paraphrase external source material where possible.

## Content hash rule

Every future ingestion process should calculate a content hash for source files or chunks.

The hash should help identify when source content has changed.

Changed source content should return to NEEDS_REVIEW until re-approved.

## Review workflow

New source entries start as DRAFT.

Reviewed but not approved source entries should use NEEDS_REVIEW.

Approved source entries use APPROVED.

Rejected source entries use REJECTED.

Archived source entries use Status ARCHIVED.

Only APPROVED, active and ACTIVE-status source entries can be used by retrieval.

## Safety review questions

Does this source belong in the approved taxonomy?

Is the source current enough for its intended use?

Is the source official, internal-approved or otherwise trusted?

Does the source contain legal, medical or decision-making claims that need safety wording?

Could this source cause the AI to overstate entitlement, diagnosis, impairment, compensation or outcome?

Does the source need a disclaimer or restricted retrieval use?

## External source rules

External source summaries must include citation labels.

External source summaries must not be copied wholesale.

External source summaries must be reviewed before approval.

External source summaries must not be used if the source is outdated or uncertain.

## Internal template rules

Internal templates must be reviewed before approval.

Internal templates must include preparation-only wording where relevant.

Internal templates must not suggest legal advice.

Internal templates must not suggest medical advice.

Internal templates must not guarantee DVA outcomes.

Internal templates must not pressure doctors or support people.

## Retrieval safety rules

Retrieval must not invent citations.

Retrieval must not use unknown sources.

Retrieval must not use draft sources.

Retrieval must not use inactive sources.

Retrieval must not use archived sources.

Retrieval must return source references with every response.

## Prompt-building safety rules

Prompt-building must include safety guardrails.

Prompt-building must include relevant source references.

Prompt-building must include active workspace data only.

Prompt-building must exclude archived conditions, evidence, evidence gaps, question responses, AI drafts and generated documents.

Prompt-building must not instruct the model to provide legal advice, medical advice, DVA decisions, impairment estimates, compensation estimates or claim outcome guarantees.

## Completion status

Created for Phase 7 ingestion and metadata control.

## Next task

Build RAG retrieval API.
