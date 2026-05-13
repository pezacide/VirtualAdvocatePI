# Phase 5 GARP M-Aware Question Engine Documentation

## App

Virtual Advocate PI

## Phase

Phase 5 - GARP M-aware question engine

## Purpose

Phase 5 adds a GARP M-aware question engine to help users organise condition information, symptoms, treatment history, stability, functional impact, lifestyle impact, work impact, worsening history, previous compensation history, evidence gaps and appointment preparation notes.

The feature is designed as a guided preparation workflow, not a formal assessment or scoring tool.

## Safety boundary

The Phase 5 tools are preparation support only.

They do not calculate GARP M impairment points.

They do not estimate compensation.

They do not provide legal advice.

They do not provide medical advice.

They do not make a DVA decision.

They do not confirm service connection.

They do not guarantee a claim outcome.

## User-facing routes

/claim-workspaces/[workspaceId]/garp-m-questions

/claim-workspaces/[workspaceId]/garp-m-summary

## Main files

web/src/lib/garpM/questionTemplateModel.ts

web/src/lib/garpM/questionGroupStructure.ts

web/src/lib/garpM/questionTemplates/diagnosisSymptomsTreatment.ts

web/src/lib/garpM/questionTemplates/stabilityTreatmentResponse.ts

web/src/lib/garpM/questionTemplates/functionalLifestyleWorkImpact.ts

web/src/lib/garpM/questionTemplates/worseningPreviousCompensation.ts

web/src/lib/garpM/questionTemplates/evidenceAppointmentPrep.ts

web/src/components/garpM/GarpMQuestionRenderer.tsx

web/src/components/garpM/GarpMQuestionEnginePanel.tsx

web/src/components/garpM/GarpMStructuredSummaryPanel.tsx

web/src/components/garpM/GarpMWorkspaceLinks.tsx

web/src/components/WorkspaceConditionNames.tsx

web/src/components/DatePickerInput.tsx

## Question groups

Diagnosis, symptoms and treatment

Stability and treatment response

Functional, lifestyle and work impact

Worsening and previous compensation

Evidence gaps and appointment preparation

Structured summary

## Backend APIs reused

GET /api/v1/claim-workspaces/{workspaceId}/conditions

GET /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

POST /api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses

## Question response mapping

GARP M question keys are saved with a garp_m: prefix.

GARP M UI sections are not sent directly as backend question groups.

Each question is mapped to a backend-safe question group based on its evidence category.

EVIDENCE_GAP questions currently map to EVIDENCE_MISSING.

APPOINTMENT_PREP questions currently map to CLAIM_CONTEXT.

PREVIOUS_COMPENSATION questions map to PREVIOUS_COMPENSATION.

## Current behaviour

Users can select a workspace and condition.

Users can answer structured GARP M-aware questions.

Users can save answers section by section.

Saved answers reload when returning to the same workspace and condition.

The question engine shows progress, saved counts, required missing counts and last saved time.

The user can continue to the next incomplete section.

Validation prompts are shown for required and short answers.

Date fields show visible Choose date buttons.

The structured summary page groups saved answers by section.

The structured summary page lists missing required answers.

The structured summary page provides a copyable plain-English preparation summary.

Copy summary gives visible feedback.

The workspace detail page links to the question engine and summary page.

The dashboard workspace cards show condition names.

## Related web usability fixes

Condition intake diagnosis status values were aligned to backend allowed values.

Condition intake date picker control was improved.

Remaining web date picker controls were standardised.

Login mode labels were clarified as Existing account and New account.

Structured summary copy feedback was improved.

Dashboard workspace cards now show condition names.

## Current limitations

Saving the same GARP M question again creates a new question response record.

The newest saved response is used when answers are reloaded.

Validation is frontend guidance only.

The current question engine is template-driven in the web app.

The question templates are not yet managed from the backend.

The structured summary is generated in the browser from saved answers.

Live AI generation is not connected to the GARP M summary yet.

DOCX or PDF export from the GARP M summary is not connected yet.

## Recommended future improvements

Add backend upsert support for question responses.

Move question templates to backend-managed versioned templates if needed.

Add export to DOCX or PDF for the structured summary.

Add AI-assisted draft generation from the structured summary.

Add condition-specific question packs.

Add more detailed evidence gap linking between question answers and evidence metadata.

Add automated route tests for Phase 5 web flows.

Add accessibility review for keyboard use, screen readers and colour contrast.

## Smoke test checklist

docs/phase-5/PHASE_5_SMOKE_TEST_CHECKLIST.md

## Completion note

Phase 5 is complete when the smoke test checklist passes, the Phase 5 documentation is committed, and the milestone is marked complete in ProjectLibre.
