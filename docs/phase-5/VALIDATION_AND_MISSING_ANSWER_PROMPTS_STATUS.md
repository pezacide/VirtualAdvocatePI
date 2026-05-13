# Validation and Missing Answer Prompts Status

## App

Virtual Advocate PI

## Phase

Phase 5 - GARP M-aware question engine

## Task

Add validation and missing-answer prompts

## Status

Completed.

## Files updated

web/src/components/garpM/GarpMQuestionRenderer.tsx

web/src/components/garpM/GarpMQuestionEnginePanel.tsx

## Current behaviour

Required GARP M-aware questions are visibly flagged when unanswered.

Question validation rules can display plain-English messages.

MIN_LENGTH and MAX_LENGTH validation rules are supported in the reusable renderer.

Section cards show validation issue counts.

The save section panel shows validation issue counts for the current section.

Save and resume behaviour remains unchanged.

## Current limitation

Validation is currently frontend guidance only.

The backend still accepts saved question responses according to its existing API rules.

## Safety boundary

Validation prompts support structured preparation only.

They do not calculate impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Build structured assessment summary screen.
