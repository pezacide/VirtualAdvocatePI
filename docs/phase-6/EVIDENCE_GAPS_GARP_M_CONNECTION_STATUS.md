# Evidence Gaps to GARP M Question Answers Status

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Connect evidence gaps to GARP M question answers

## Status

Completed.

## Files added or updated

backend/src/VirtualAdvocatePI.Api/Features/Evidence/EvidenceGapEndpoints.cs

web/src/lib/evidenceGapUi.ts

## Current behaviour

The backend evidence gap recalculation flow now reads saved GARP M question responses for the selected workspace and condition.

GARP M answers can influence medication, functional impact, previous compensation, worsening and general evidence follow-up gap prompts.

GARP M question answers are read from saved question response records using garp_m question keys.

The frontend now includes a friendly label for the general GARP M evidence follow-up gap.

## Backend behaviour

No database migration was required.

No new endpoint was required.

The existing evidence gap recalculation endpoint was extended.

Existing evidence item, accepted-condition history and condition rules remain in place.

## Safety boundary

GARP M-linked evidence gaps are preparation prompts only.

They do not calculate impairment, decide whether evidence is sufficient, prove service connection, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Connect evidence gaps to condition intake and accepted history.
