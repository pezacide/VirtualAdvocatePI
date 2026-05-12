# Question Group Structure Status

## App

Virtual Advocate PI

## Phase

Phase 5 - GARP M-aware question engine

## Task

Create GARP M-aware question group structure

## Status

Completed.

## Files added or updated

web/src/lib/garpM/questionGroupStructure.ts

web/src/lib/garpM/index.ts

## Question groups created

Diagnosis, symptoms and treatment

Stability and treatment response

Functional, lifestyle and work impact

Worsening and previous compensation

Evidence gaps and appointment preparation

Structured summary

## Technical notes

The group structure defines route segments, display order, titles, descriptions, why-this-matters text and safety notes.

The current template set contains empty question arrays.

The next tasks will populate these groups with question templates.

## Safety boundary

The question group structure supports GARP M-aware evidence capture only.

It does not support formal GARP M scoring.

It does not calculate impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Build reusable question renderer.
