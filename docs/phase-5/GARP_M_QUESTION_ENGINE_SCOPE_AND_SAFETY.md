# GARP M-Aware Question Engine Scope and Safety Boundary

## App

Virtual Advocate PI

## Phase

Phase 5 - GARP M-aware question engine

## Purpose

The Phase 5 question engine will guide users through structured, plain-English questions that help organise condition information, symptoms, treatment history, stability, functional impact, lifestyle impact, work impact, worsening history, previous compensation history and evidence gaps.

The feature is intended to improve evidence preparation and appointment preparation before a user speaks with a doctor, advocate, lawyer or support person.

## Core product boundary

The question engine is GARP M-aware, not a formal GARP M scorer.

It may ask structured questions that are broadly aligned with impairment-related information gathering.

It may help identify missing information.

It may produce a plain-English structured summary of user-provided answers.

It may help the user prepare questions for a doctor, advocate, lawyer or support person.

It must not calculate impairment points.

It must not estimate compensation.

It must not make a medical finding.

It must not make a legal finding.

It must not predict a DVA outcome.

It must not tell the user that a claim will succeed.

It must not replace professional advice from a doctor, advocate, lawyer or DVA decision-maker.

## Allowed behaviour

The question engine may:

Ask condition-specific questions.

Ask diagnosis, symptoms and treatment questions.

Ask stability and treatment response questions.

Ask functional, lifestyle and work impact questions.

Ask worsening and previous compensation history questions.

Ask evidence availability questions.

Save and resume user answers.

Show missing-answer prompts.

Show evidence gap prompts.

Generate a structured plain-English summary.

Suggest appointment preparation questions.

Label information as user-provided.

Use plain veteran-friendly language.

Explain why a question is being asked.

## Disallowed behaviour

The question engine must not:

Score the user under GARP M.

Allocate impairment points.

Tell the user which impairment rating applies.

Estimate the user's compensation.

Say that DVA will accept or reject a claim.

Say that a condition is service-related.

Diagnose a condition.

Recommend medical treatment.

Tell a user to start, stop or change medication.

Provide legal advice.

Present generated text as final evidence without review.

Hide uncertainty.

## Required safety wording

Every Phase 5 screen should retain the preparation-support boundary in plain language.

Recommended wording:

This feature helps organise information for preparation only. It does not calculate GARP M impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## User experience principles

The question engine should be veteran-friendly and practical.

Questions should be short and plain-English.

Each section should explain why the information may be useful.

The user should be able to skip questions where appropriate.

The user should be able to save and resume later.

The user should see what information is missing before generating a summary.

The user should be able to review and edit answers before using them in any draft or document.

## Phase 5 question sections

The first Phase 5 version should use these sections:

Diagnosis, symptoms and treatment

Stability and treatment response

Functional, lifestyle and work impact

Worsening and previous compensation

Evidence gaps and appointment preparation

Structured summary

## Technical direction

The first web route should be:

/claim-workspaces/[workspaceId]/garp-m-questions

The question engine should reuse the existing backend question response API where possible.

The question template model should live in the web app first, then be moved to the backend later if needed.

The feature should remain modular so future scoring logic, if ever added, can be separated from the current evidence-capture workflow.

## Phase 5 success criteria

Phase 5 is complete when:

The scope and safety boundary is documented.

A question template model exists.

A reusable question renderer exists.

The main question groups are available.

Answers can be saved and resumed.

Missing answers can be highlighted.

A structured plain-English summary can be generated from saved answers.

The workspace detail page links to the GARP M-aware question engine.

Smoke test documentation is updated.

Preparation-only safety wording remains visible.
