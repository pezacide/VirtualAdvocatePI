# Phase 8 Claim Starter Pack Document Generation Plan

## App

Virtual Advocate PI

## Phase

Phase 8 - Claim Starter Pack document generation

## Purpose

Phase 8 adds reviewed-document generation for the Claim Starter Pack workflow.

The goal is to generate downloadable DOCX and later PDF claim preparation packs using reviewed workspace content only.

## Phase 8 source-of-truth task list

Design DOCX starter pack templates.

Build document generation service.

Add PDF conversion and storage versioning.

Add document download signed URL flow.

Enforce reviewed-only content and test exports.

Milestone: Phase 8 complete.

## Safety boundary

The generated pack is preparation support only.

It does not submit anything to DVA.

It does not provide legal advice.

It does not provide medical advice.

It does not make DVA decisions.

It does not calculate impairment points.

It does not estimate compensation.

It does not guarantee claim outcomes.

The user must review all content before using it.

## Reviewed-only rule

Generated documents should only include content that has been reviewed, approved or explicitly confirmed by the user.

AI draft material should only be included when ReviewStatus is APPROVED.

Archived workspaces, archived conditions, archived evidence, archived gaps, archived question responses and archived AI drafts must be excluded.

Evidence files that have had uploaded file content deleted must not be treated as uploaded files.

## Starter pack sections

Cover page.

Preparation-only safety note.

Workspace summary.

Conditions included.

Accepted-condition history summary.

Approved AI draft summaries.

Evidence list.

Evidence gap summary.

GARP M-aware preparation summary.

Doctor appointment questions.

Follow-up checklist.

Review and sign-off page.

## Initial output formats

DOCX first.

PDF later after DOCX generation is stable.

## Data sources

Active workspace metadata.

Active conditions.

Active accepted-condition history.

Active reviewed question responses.

Active evidence metadata.

Active evidence gaps.

Approved AI drafts only.

Generated document metadata.

Workspace audit trail for internal tracking.

## Phase 8 implementation approach

First define the DOCX template structure.

Then build a backend document generation service.

Then store generated document metadata and file versions.

Then add download signed URL support.

Then enforce reviewed-only content and run export smoke tests.

## Current status

Planning started.

## Next task

Design DOCX starter pack templates.
