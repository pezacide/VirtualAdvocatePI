# Evidence Gap Rules Engine Status

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Build evidence gap rules engine

## Status

Completed.

## Files added or updated

web/src/lib/evidenceGapUi.ts

web/src/components/EvidenceGapTrackerPanel.tsx

## Backend finding

The backend already has a working evidence gap rules engine.

The existing backend recalculates condition-specific evidence gaps, archives previous active gaps, creates new plain-English preparation prompts, and supports status updates.

## Current behaviour

The frontend now presents backend gap types, severities and statuses using friendly labels.

The gap tracker uses backend-valid statuses only: Open, In progress, Resolved and Not applicable.

The gap summary now includes Open, In progress and Resolved counts.

Recalculate evidence gaps continues to use the existing backend endpoint.

## Safety boundary

Evidence gaps are preparation prompts only.

They do not decide whether evidence is sufficient, prove service connection, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Build gap dashboard and reminder prompts.
