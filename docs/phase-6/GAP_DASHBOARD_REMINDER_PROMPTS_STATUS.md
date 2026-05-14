# Gap Dashboard and Reminder Prompts Status

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Build gap dashboard and reminder prompts

## Status

Completed.

## Files added or updated

web/src/components/EvidenceGapReminderPanel.tsx

web/src/components/EvidenceGapTrackerPanel.tsx

## Current behaviour

The evidence gap tracker now shows a gap dashboard for the selected condition.

The dashboard shows workspace gap count, open condition gap count, high-priority gap count and in-progress gap count.

The dashboard generates practical reminder prompts based on open, high-priority, in-progress and resolved gaps.

High-priority gaps are highlighted in a focused list.

The existing backend evidence gap rules engine remains unchanged.

Recalculate and gap status update behaviour remain unchanged.

## Safety boundary

Gap reminder prompts are preparation support only.

They do not decide whether evidence is sufficient, prove service connection, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Connect evidence gaps to GARP M question answers.
