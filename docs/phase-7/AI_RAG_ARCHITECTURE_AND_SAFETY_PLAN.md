# AI/RAG Architecture and Safety Plan

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Create AI/RAG architecture and safety plan

## Status

Drafted.

## Purpose

Phase 7 adds an AI/RAG-assisted preparation layer to Virtual Advocate PI.

The AI/RAG workflow will help transform active workspace data and approved reference material into plain-English preparation drafts.

The veteran remains in control of all draft content.

The workflow must be reviewable, auditable, source-aware and safety-bounded.

## Core principle

The app is a preparation and organisation tool.

It does not provide legal advice.

It does not provide medical advice.

It does not diagnose conditions.

It does not calculate impairment points.

It does not estimate compensation.

It does not make DVA decisions.

It does not submit anything to DVA.

It does not guarantee claim success.

## AI/RAG architecture overview

1. Approved source registry

Only approved source records can be used by retrieval and prompt-building workflows.

Each source must have metadata such as source name, category, version/date, jurisdiction, source type, storage path, citation label and active status.

2. Knowledge base structure

The knowledge base stores approved reference content, internal templates and safe drafting guidance.

Content must be split into small, labelled, retrievable source chunks.

3. Source category taxonomy

Sources are grouped by purpose, such as GARP M guidance, DVA reform references, internal templates, safety wording, evidence prompts and doctor-question templates.

4. Ingestion and metadata rules

Every source chunk must keep its source reference, category, version/date and review status.

Drafts must be able to show what source material was used.

5. RAG retrieval API

The backend will retrieve relevant active source chunks for a selected draft task, workspace and condition.

Retrieval responses must include source references and citation labels.

6. Prompt templates and guardrails

Prompt templates must use approved source material, active workspace data and strict safety wording.

Prompts must instruct the model not to provide legal advice, medical advice, DVA decisions, impairment estimates or outcome guarantees.

7. AI draft request API

The draft API will generate preparation drafts for supported draft types.

Generated drafts are saved as reviewable AI draft records.

8. Review, copy and save workflow

AI drafts must be reviewed by the user before use.

Users can copy, edit, approve, reject, regenerate or archive drafts.

Only approved drafts should be available for generated document inclusion.

9. Audit logging

AI/RAG actions must create audit events for retrieval, draft creation, draft update, approval, rejection, regeneration and archive actions.

## Supported initial draft types

Veteran statement.

Worsening summary.

Doctor appointment questions.

Evidence gap summary.

Doctor request letter.

## Workspace data allowed in drafting

Active workspace details.

Active condition intake data.

Active accepted-condition history.

Active GARP M question responses.

Active evidence item metadata.

Active evidence gap tracker output.

Approved source registry entries.

Approved knowledge base chunks.

Approved internal templates.

## Workspace data excluded from drafting

Archived workspaces.

Archived conditions.

Archived evidence.

Archived evidence gaps.

Archived question responses.

Archived AI drafts.

Deleted uploaded files where StoragePath and UploadedAt have been cleared.

Any source registry item not marked active and approved.

## Source safety rules

Do not use unapproved source files.

Do not use unknown source material.

Do not invent citations.

Do not present generated text as a DVA decision.

Do not state that a claim will be accepted.

Do not state that evidence is legally or medically sufficient.

Do not calculate compensation or impairment points.

Do not diagnose the user.

Do not tell the user to stop, start or change treatment.

## Draft safety wording

All AI-assisted outputs should be labelled as preparation support only.

Drafts should say they are for review and editing by the user.

Drafts should avoid final legal, medical or entitlement conclusions.

Drafts should use plain Australian English.

Drafts should be respectful, practical and veteran-friendly.

## Citation/source-reference requirement

RAG retrieval responses must return source references.

AI drafts should store sourceReferences.

Draft review screens should show source references where available.

Generated document workflows should prefer approved AI drafts with saved sourceReferences.

## Audit event plan

AI_SOURCE_REGISTRY_CREATED.

AI_SOURCE_REGISTRY_UPDATED.

AI_KNOWLEDGE_SOURCE_ADDED.

AI_KNOWLEDGE_SOURCE_ARCHIVED.

AI_RAG_RETRIEVAL_REQUESTED.

AI_DRAFT_REQUESTED.

AI_DRAFT_CREATED.

AI_DRAFT_UPDATED.

AI_DRAFT_APPROVED.

AI_DRAFT_REJECTED.

AI_DRAFT_REGENERATED.

AI_DRAFT_ARCHIVED.

## Phase 7 task order

1. Create AI/RAG architecture and safety plan.

2. Create approved source registry schema.

3. Create approved source registry and knowledge base structure.

4. Create source category taxonomy.

5. Load GARP M, DVA reform references and internal templates.

6. Create knowledge base seed content files.

7. Create ingestion and metadata rules.

8. Build RAG retrieval API.

9. Build retrieval response with citations/source references.

10. Build prompt templates and safety guardrails.

11. Build AI draft request API.

12. Generate veteran statement and worsening summary drafts.

13. Generate doctor questions, evidence gap summary and doctor letter.

14. Add AI draft review, copy and save workflow.

15. Add AI/RAG audit logging.

16. Add AI/RAG smoke test checklist.

17. Milestone: Phase 7 complete.

## Implementation approach

Build the source registry and knowledge base structure before enabling generation.

Build retrieval and citation/source-reference output before prompt generation.

Build draft review and approval before allowing generated drafts into generated documents.

Keep all AI/RAG actions auditable.

## Completion criteria for this task

This plan exists in docs/phase-7.

The safety boundaries are documented.

The intended AI/RAG architecture is documented.

The task order matches ProjectLibre Phase 7.

## Next task

Create approved source registry schema.
