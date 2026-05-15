# Doctor Guidance Pack Export Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 9 - Doctor guidance pack

## Task

Export and test doctor guidance pack

## Safety boundary

[ ] The pack says preparation support only.
[ ] The pack does not provide medical advice.
[ ] The pack does not provide legal advice.
[ ] The pack does not diagnose conditions.
[ ] The pack does not tell a doctor what opinion to provide.
[ ] The pack does not pressure a doctor to support a claim.
[ ] The pack does not ask a doctor to make a DVA decision.
[ ] The pack does not calculate impairment points.
[ ] The pack does not estimate compensation.
[ ] The pack does not guarantee claim outcomes.

## Reviewed-only checks

[ ] Only active workspace records are included.
[ ] Only approved doctor guidance AI drafts are included.
[ ] USER_REVIEW_REQUIRED doctor guidance drafts are excluded.
[ ] USER_EDITED doctor guidance drafts are excluded unless approved.
[ ] REJECTED doctor guidance drafts are excluded.
[ ] ARCHIVED doctor guidance drafts are excluded.

## Export checks

[ ] Generate Doctor Guidance Pack.
[ ] Generated document record appears.
[ ] DOCX path is populated.
[ ] PDF path is populated.
[ ] Download DOCX works.
[ ] Download PDF works.
[ ] DOCX includes doctor-specific disclaimer.
[ ] PDF includes doctor-specific disclaimer.
[ ] DOCX includes reviewed-only wording.
[ ] PDF includes reviewed-only wording.

## Audit checks

[ ] DOCTOR_GUIDANCE_PACK_CREATED appears in audit trail.
[ ] DOCTOR_GUIDANCE_PACK_REVIEWED_ONLY_ENFORCED appears in audit trail.
[ ] GENERATED_DOCUMENT_DOWNLOAD_URL_CREATED appears after download.

## Close-out

[ ] Backend build passes.
[ ] Web build passes.
[ ] Manual export test passes.
