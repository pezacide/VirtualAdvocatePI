# Claim Starter Pack Export Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 8 - Claim Starter Pack document generation

## Task

Enforce reviewed-only content and test exports

## Purpose

This checklist confirms that Claim Starter Pack exports include reviewed content only and produce downloadable DOCX and PDF files.

## Safety boundary

The generated pack is preparation support only.

It does not submit anything to DVA.

It does not provide legal advice.

It does not provide medical advice.

It does not make DVA decisions.

It does not calculate impairment points.

It does not estimate compensation.

It does not guarantee claim outcomes.

## Pre-test setup

[ ] Backend builds successfully.

[ ] Web app builds successfully.

[ ] Backend is deployed to Cloud Run if testing against Cloud Run.

[ ] Web .env.local points to the intended backend.

[ ] User can sign in.

[ ] Test workspace exists.

[ ] Test workspace has at least one active condition.

[ ] Test workspace has at least one active evidence item or evidence gap where possible.

[ ] Test workspace has at least one APPROVED AI draft.

[ ] Test workspace has at least one non-approved AI draft to confirm exclusion.

## Reviewed-only inclusion checks

[ ] Generated pack includes active workspace records.

[ ] Generated pack includes active conditions only.

[ ] Generated pack includes active evidence metadata only.

[ ] Generated pack includes active evidence gaps only.

[ ] Generated pack includes APPROVED AI drafts only.

[ ] Generated pack excludes USER_REVIEW_REQUIRED AI drafts.

[ ] Generated pack excludes USER_EDITED AI drafts unless later marked APPROVED.

[ ] Generated pack excludes REJECTED AI drafts.

[ ] Generated pack excludes ARCHIVED AI drafts.

[ ] Generated pack displays reviewed-only inclusion wording.

## DOCX export checks

[ ] Click Generate Claim Starter Pack.

[ ] Generated document record appears.

[ ] Document status is GENERATED or DOWNLOADED after opening a download link.

[ ] DOCX storage path is populated.

[ ] DOCX download button is enabled.

[ ] DOCX signed URL opens or downloads the file.

[ ] DOCX contains the preparation-only safety note.

[ ] DOCX contains reviewed-only inclusion wording.

[ ] DOCX contains approved AI draft content only.

## PDF export checks

[ ] PDF storage path is populated.

[ ] PDF download button is enabled.

[ ] PDF signed URL opens or downloads the file.

[ ] PDF contains the preparation-only safety note.

[ ] PDF contains reviewed-only inclusion wording.

[ ] PDF contains approved AI draft content only.

## Versioning checks

[ ] First generated pack has version label such as v001.

[ ] Second generated pack creates a new version label such as v002.

[ ] DOCX and PDF storage paths include the version label.

[ ] GeneratedDocument TemplateVersion includes the version label.

## Audit checks

[ ] GENERATED_DOCUMENT_CREATED appears in audit trail.

[ ] CLAIM_STARTER_PACK_DOCX_GENERATED appears in audit trail.

[ ] CLAIM_STARTER_PACK_PDF_GENERATED appears in audit trail.

[ ] CLAIM_STARTER_PACK_VERSION_CREATED appears in audit trail.

[ ] CLAIM_STARTER_PACK_REVIEWED_ONLY_ENFORCED appears in audit trail.

[ ] GENERATED_DOCUMENT_DOWNLOAD_URL_CREATED appears after opening DOCX or PDF.

## Close-out criteria

[ ] Backend build passes.

[ ] Web build passes.

[ ] DOCX export works.

[ ] PDF export works.

[ ] Download signed URLs work.

[ ] Reviewed-only AI draft inclusion is confirmed.

[ ] Audit events are visible.

[ ] Safety wording is visible in UI and exported files.
