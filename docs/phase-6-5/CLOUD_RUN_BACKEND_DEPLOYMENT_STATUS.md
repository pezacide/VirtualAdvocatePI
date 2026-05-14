# Cloud Run Backend Deployment Status

## App

Virtual Advocate PI

## Phase

Phase 6.5 - Removal and archive controls

## Task

Deploy updated backend to Cloud Run

## Status

Completed.

## Service

vapi-dev-api

## Purpose

Deploy the backend update that adds the uploaded evidence file deletion endpoint.

## Endpoint deployed

DELETE /api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/uploaded-file

## Deployment fix

The API Dockerfile was updated so Cloud Build can build from the backend API project folder.

A .gcloudignore file was added to avoid uploading bin, obj and backup files.

## Expected behaviour

Deleting an uploaded file removes the stored file from app storage.

The evidence item remains listed in the workspace.

The evidence item returns to Listed, not uploaded.

The Open file action becomes unavailable.

The audit trail records EVIDENCE_UPLOADED_FILE_DELETED.

## Safety boundary

Deleting an uploaded file only removes the file from this app storage.

It does not contact DVA.

It does not remove anything already submitted outside this app.

It does not make a DVA decision, provide legal advice, provide medical advice, or guarantee any claim outcome.
