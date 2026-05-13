# Structured Assessment Summary Status

## App

Virtual Advocate PI

## Phase

Phase 5 - GARP M-aware question engine

## Task

Build structured assessment summary screen

## Status

Completed.

## Files added or updated

web/src/components/garpM/GarpMStructuredSummaryPanel.tsx

web/src/components/garpM/index.ts

web/src/app/claim-workspaces/[workspaceId]/garp-m-summary/page.tsx

## Current behaviour

Signed-in users can open a structured summary page for a real workspace.

The page loads conditions for the workspace.

The page loads saved GARP M-aware question responses for the selected condition.

The page groups saved answers by question section.

The page shows missing required answers.

The page generates a copyable plain-English preparation summary.

## Safety boundary

The summary is preparation support only.

It does not calculate GARP M impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Add workspace detail links to question engine.
