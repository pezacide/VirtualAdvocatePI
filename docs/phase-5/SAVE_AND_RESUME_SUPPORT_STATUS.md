# Save and Resume Support Status

## App

Virtual Advocate PI

## Phase

Phase 5 - GARP M-aware question engine

## Task

Add save and resume support

## Status

Completed.

## Files updated

web/src/components/garpM/GarpMQuestionEnginePanel.tsx

## Current behaviour

Saved GARP M-aware answers reload when the user returns to the workspace and condition.

The page shows how many sections have saved answers.

The page shows how many sections have required answers completed.

The page shows the latest saved time.

Each section card shows answered count, saved count and required missing count.

The user can continue to the next incomplete section.

Unsaved changes are flagged before the user leaves the section.

## Current limitation

Saving the same question again creates a new question response record.

The newest saved response is used when answers are reloaded.

A future backend upsert endpoint could reduce duplicate response records.

## Safety boundary

The save and resume workflow supports structured preparation only.

It does not calculate impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Add validation and missing-answer prompts.
