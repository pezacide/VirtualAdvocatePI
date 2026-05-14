# Evidence Gaps to Condition Intake and Accepted History Status

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Connect evidence gaps to condition intake and accepted history

## Status

Completed.

## Backend finding

The backend evidence gap recalculation flow already reads condition intake fields and accepted-condition history records.

Condition intake fields available to the backend include diagnosis status, date diagnosed, current symptoms, treatment summary, medication summary, medication side effects, functional impact summary, lifestyle impact summary, work impact summary, stability notes and worsening notes.

Accepted-condition history fields available to the backend include previous DVA acceptance, original Act, previous compensation, previous DVA decision letter availability, previous assessment letter availability, previous decision date, previous assessment date, worsening claimed and worsening summary.

## Existing gap connections confirmed

Medication summary can influence medication evidence prompts.

Functional impact summary can influence functional impact evidence prompts.

Previous DVA acceptance or previous compensation history can influence previous DVA decision letter and previous assessment evidence prompts.

Worsening notes or accepted-history worsening fields can influence worsening evidence prompts.

Evidence item records are checked before creating evidence gap prompts.

Accepted-condition history records are checked before creating previous DVA or worsening-related prompts.

## Decision

No new backend endpoint was required.

No database migration was required.

No additional backend rewrite was required for this task.

The task is closed based on confirmed existing backend connections and the previous GARP M gap-rule extension.

## Safety boundary

Evidence gaps linked to condition intake and accepted-history fields are preparation prompts only.

They do not decide whether evidence is sufficient, prove service connection, confirm DVA acceptance, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Add evidence upload validation and error handling.
