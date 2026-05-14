# AI/RAG Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 7 - AI/RAG knowledge base

## Task

Add AI/RAG smoke test checklist

## Purpose

This checklist confirms that the Phase 7 AI/RAG knowledge base, retrieval, draft generation, review workflow and audit logging operate safely.

## Safety boundary

AI/RAG features are preparation support only.

They do not submit anything to DVA.

They do not provide legal advice.

They do not provide medical advice.

They do not diagnose conditions.

They do not calculate impairment points.

They do not estimate compensation.

They do not make DVA decisions.

They do not guarantee claim outcomes.

## Pre-test setup

[ ] Backend builds successfully.

[ ] Web app builds successfully.

[ ] Backend is deployed to Cloud Run if testing against Cloud Run.

[ ] Web .env.local points to the intended backend.

[ ] User can sign in.

[ ] Test workspace exists.

[ ] Test condition exists.

[ ] Test condition has at least one evidence item or evidence gap where possible.

[ ] Workspace audit trail is accessible.

## Knowledge base file checks

[ ] knowledge-base/source-registry/approved-source-registry.loaded.seed.json exists.

[ ] knowledge-base/source-registry/source-category-taxonomy.json exists.

[ ] knowledge-base/seed-content/knowledge-base-chunks.seed.jsonl exists.

[ ] knowledge-base/prompt-templates/prompt-template-manifest.json exists.

[ ] backend/src/VirtualAdvocatePI.Api/KnowledgeBase/seed-content/knowledge-base-chunks.seed.jsonl exists.

[ ] backend/src/VirtualAdvocatePI.Api/KnowledgeBase/source-registry/approved-source-registry.loaded.seed.json exists.

[ ] backend/src/VirtualAdvocatePI.Api/KnowledgeBase/prompt-templates exists.

## Local validation scripts

[ ] knowledge-base/ingestion/Test-KnowledgeBaseSeedContent.ps1 passes.

[ ] knowledge-base/prompt-templates/Test-PromptTemplates.ps1 passes.

## RAG retrieval API smoke test

[ ] POST /api/v1/claim-workspaces/{workspaceId}/ai-rag/retrieve accepts a valid request.

[ ] Retrieval returns approved active chunks only.

[ ] Retrieval response includes returnedChunkCount.

[ ] Retrieval response includes citationCount.

[ ] Retrieval response includes citations.

[ ] Retrieval response includes sourceReferences.

[ ] Every returned chunk has a citationMarker.

[ ] Every citationMarker resolves to a citation/source reference.

[ ] Retrieval response includes safety flags.

[ ] Workspace audit trail records AI_RAG_RETRIEVAL_REQUESTED.

## AI draft request API smoke test

[ ] POST /api/v1/claim-workspaces/{workspaceId}/ai-drafts/request accepts a valid request.

[ ] Request package includes active workspace data.

[ ] Request package includes selected condition data.

[ ] Request package excludes archived conditions, evidence, gaps and question responses.

[ ] Request package includes source references.

[ ] Request package includes shared safety guardrails.

[ ] Request package includes task-specific prompt template text.

[ ] Response safety flags show aiGenerationEnabled false.

[ ] Workspace audit trail records AI_DRAFT_REQUESTED.

## AI draft generation smoke test

[ ] Generate VETERAN_STATEMENT draft.

[ ] Generated draft is saved as an AiDraft record.

[ ] Draft ReviewStatus is USER_REVIEW_REQUIRED.

[ ] Draft Status is ACTIVE.

[ ] Draft SourceReferences are saved.

[ ] Draft includes preparation-only wording.

[ ] Draft does not claim to be legal advice, medical advice or a DVA decision.

[ ] Workspace audit trail records AI_DRAFT_CREATED.

[ ] Generate WORSENING_SUMMARY draft.

[ ] Generate DOCTOR_QUESTIONS draft.

[ ] Generate EVIDENCE_GAP_SUMMARY draft.

[ ] Generate DOCTOR_REQUEST_LETTER draft.

[ ] All generated drafts require user review.

## AI draft review workflow smoke test

[ ] AI drafts page loads.

[ ] Condition selector loads active conditions.

[ ] Generate reviewable draft button works.

[ ] Saved drafts list refreshes.

[ ] Draft can be selected.

[ ] Draft text appears in review editor.

[ ] Draft text can be copied.

[ ] Draft text can be edited.

[ ] Save draft review works.

[ ] ReviewStatus can be changed to USER_EDITED.

[ ] Approve button changes ReviewStatus to APPROVED.

[ ] Reject button changes ReviewStatus to REJECTED.

[ ] Archive button archives the draft.

[ ] Archived draft disappears from active draft list after refresh.

## AI/RAG audit logging smoke test

[ ] AI_RAG_RETRIEVAL_REQUESTED appears in workspace audit trail.

[ ] AI_DRAFT_REQUESTED appears in workspace audit trail.

[ ] AI_DRAFT_CREATED appears in workspace audit trail.

[ ] AI_DRAFT_UPDATED appears in workspace audit trail.

[ ] AI_DRAFT_USER_EDITED appears in workspace audit trail when review text is edited and saved.

[ ] AI_DRAFT_APPROVED appears in workspace audit trail when a draft is approved.

[ ] AI_DRAFT_REJECTED appears in workspace audit trail when a draft is rejected.

[ ] AI_DRAFT_ARCHIVED appears in workspace audit trail when a draft is archived.

## Safety wording smoke test

[ ] AI drafts page displays preparation-support-only wording.

[ ] Generated drafts include a review note.

[ ] Generated drafts do not guarantee claim outcomes.

[ ] Generated drafts do not calculate impairment points.

[ ] Generated drafts do not estimate compensation.

[ ] Generated drafts do not diagnose conditions.

[ ] Generated drafts do not claim to submit anything to DVA.

## Close-out criteria

[ ] Backend build passes.

[ ] Web build passes.

[ ] Knowledge base validation passes.

[ ] Prompt template validation passes.

[ ] Retrieval endpoint works.

[ ] Draft request endpoint works.

[ ] Draft generation endpoint works for all supported Phase 7 draft types.

[ ] Draft review/copy/save/approve/reject/archive workflow works.

[ ] Audit trail records AI/RAG events.

[ ] Safety wording is visible and accurate.

## Status

Checklist created.

## Next task

Milestone: Phase 7 complete.
