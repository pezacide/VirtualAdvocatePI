# Condition Archive and Remove Flow Status

## App

Virtual Advocate PI

## Phase

Phase 6.5 - Removal and archive controls

## Task

Build condition archive/remove flow

## Status

Completed.

## Backend finding

The backend already had a DELETE condition endpoint.

The endpoint archives a condition by setting Status to ARCHIVED.

Active condition list endpoints already exclude archived conditions.

The backend already writes a CONDITION_ARCHIVED audit event.

No backend endpoint or database migration was required for this task.

## Files added or updated

web/src/lib/api/conditions.ts

web/src/components/ConditionIntakePanel.tsx

## Current behaviour

Condition intake cards now include a Remove from workspace action.

The action asks for confirmation before removing the condition from active workspace tools.

Removed conditions are archived rather than hard deleted.

Archived conditions disappear from active condition lists after reload.

Archived conditions are excluded from condition dropdowns used by evidence upload, metadata, GARP M questions, evidence gaps and future AI draft preparation because those screens use the active condition list.

The audit trail records the CONDITION_ARCHIVED event.

## Safety boundary

Removing a condition from the workspace does not contact DVA.

It does not remove anything already submitted outside this app.

It does not hard delete linked historical records.

It does not make a DVA decision, provide legal advice, provide medical advice, or guarantee any claim outcome.

## Next task

Ensure archived evidence is excluded from gaps and AI drafts.
