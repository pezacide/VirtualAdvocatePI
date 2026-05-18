# Question and Document Template Editor Status

## App

Virtual Advocate PI

## Phase

Phase 10 - Admin knowledge and template manager

## Task

Build question and document template editor

## Status

Completed.

## Backend files created

backend/src/VirtualAdvocatePI.Api/Domain/Admin/AdminTemplateRegistryEntry.cs.

backend/src/VirtualAdvocatePI.Api/Features/Admin/AdminTemplateRegistryEndpoints.cs.

## Backend files updated

backend/src/VirtualAdvocatePI.Api/Data/VirtualAdvocateDbContext.cs.

backend/src/VirtualAdvocatePI.Api/Program.cs.

## Database migration

AddAdminTemplateRegistryEntries.

## Frontend files created

web/src/lib/api/adminTemplates.ts.

web/src/components/AdminTemplateEditorPanel.tsx.

## Frontend files updated

web/src/lib/api/index.ts.

web/src/app/admin/templates/questions/page.tsx.

web/src/app/admin/templates/documents/page.tsx.

## Backend endpoints

GET /api/v1/admin/templates.

POST /api/v1/admin/templates.

PATCH /api/v1/admin/templates/{id}.

## Behaviour

Admins can create question templates.

Admins can create document templates.

Admins can filter templates by search, type, category, approval status and status.

Admins can edit template title, description, category, version, body, output format, approval status, active flag, status and review notes.

TemplateKey is locked after creation and treated as a stable identifier.

Normal users cannot access the admin template endpoints.

## Safety boundary

Template editing is admin-only.

This task creates the admin template registry foundation.

Template versioning, duplication and stronger audit logging can be extended in later Phase 10 tasks.

## Next task

Build prompt and disclaimer versioning.
