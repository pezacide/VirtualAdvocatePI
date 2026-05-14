# Evidence Upload Validation and Error Handling Status

## App

Virtual Advocate PI

## Phase

Phase 6 - Evidence upload and gap tracker

## Task

Add evidence upload validation and error handling

## Status

Completed.

## Files added or updated

backend/src/VirtualAdvocatePI.Api/Features/Evidence/EvidenceUploadEndpoints.cs

web/src/lib/evidenceUploadValidation.ts

web/src/components/EvidenceUploadPanel.tsx

## Current behaviour

The evidence upload page now uses shared frontend file validation.

The file picker prefers supported file types.

Supported files include PDF, image, Word, text and RTF files.

The maximum upload size is 25 MB.

The frontend shows clearer validation messages for missing, empty, unsupported or oversized files.

The backend validates upload URL requests for file name, file size, file extension and content type.

Cloud Storage upload failure messages now explain that the signed link may have expired or the file may not have been accepted.

Successful uploads still create a signed upload URL, upload the file, confirm the evidence item and reload the evidence list.

## Safety boundary

Evidence upload validation is technical preparation support only.

It does not inspect clinical content, decide whether evidence is sufficient, submit files to DVA, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.

## Next task

Build evidence audit trail view.
