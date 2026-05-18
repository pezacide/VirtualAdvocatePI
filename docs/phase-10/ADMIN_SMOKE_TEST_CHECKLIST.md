# Admin Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 10 - Admin knowledge and template manager

## Purpose

This checklist confirms that the Phase 10 admin tools are protected, functional and auditable.

## Admin access

[ ] Sign in as configured admin.
[ ] Open /admin/access-check.
[ ] Confirm Role is ADMIN.
[ ] Confirm Account status is ACTIVE.
[ ] Confirm Is admin is Yes.
[ ] Confirm Protected ping succeeds.
[ ] Confirm normal users are denied admin access.

## Admin dashboard

[ ] Open /admin.
[ ] Confirm admin dashboard loads.
[ ] Confirm admin status card displays signed-in admin email.
[ ] Confirm all Phase 10 admin tool cards are visible.
[ ] Open each admin tool card.

## Source metadata manager

[ ] Open /admin/source-metadata.
[ ] Confirm source registry entries load.
[ ] Apply search filter.
[ ] Select a source entry.
[ ] Edit Review notes.
[ ] Save source metadata.
[ ] Refresh and confirm saved changes remain.
[ ] Confirm source key is not editable.

## Source registry seed and database maintenance

[ ] Open /admin/database-maintenance.
[ ] Confirm Apply migrations is admin-only.
[ ] Confirm pending migrations can be applied if needed.
[ ] Open /admin/source-registry-seed.
[ ] Confirm seed approved sources is admin-only.
[ ] Confirm running the seed twice skips existing source keys.

## Question template editor

[ ] Open /admin/templates/questions.
[ ] Create a test QUESTION template.
[ ] Edit the template body.
[ ] Save template.
[ ] Refresh and confirm changes remain.
[ ] Confirm TemplateKey is locked after creation.

## Document template editor

[ ] Open /admin/templates/documents.
[ ] Create a test DOCUMENT template.
[ ] Edit review notes.
[ ] Save template.
[ ] Refresh and confirm changes remain.
[ ] Confirm TemplateKey is locked after creation.

## Prompt and disclaimer versioning

[ ] Open /admin/prompts-disclaimers.
[ ] Create a test PROMPT version.
[ ] Create a test DISCLAIMER version.
[ ] Edit review notes.
[ ] Save changes.
[ ] Refresh and confirm changes remain.
[ ] Confirm VersionKey is locked after creation.

## Knowledge base audit review

[ ] Open /admin/knowledge-audit.
[ ] Confirm audit events load.
[ ] Confirm event type summary appears.
[ ] Click an event type summary card.
[ ] Apply filters.
[ ] Select an audit event.
[ ] Confirm selected event details are visible.
[ ] Untick knowledge/admin-relevant event types only.
[ ] Apply filters and confirm broader audit events load.

## Admin audit logging

[ ] Make a source metadata change.
[ ] Confirm ADMIN_SOURCE_REGISTRY_UPDATED appears in /admin/knowledge-audit.
[ ] Make a template change.
[ ] Confirm ADMIN_TEMPLATE_UPDATED appears in /admin/knowledge-audit.
[ ] Make a prompt/disclaimer change.
[ ] Confirm ADMIN_PROMPT_DISCLAIMER_VERSION_UPDATED appears in /admin/knowledge-audit.
[ ] Confirm request bodies are not logged.

## Safety boundary

[ ] Admin tools are not visible to unauthenticated users.
[ ] Admin tools deny non-admin users.
[ ] Admin tools do not bypass veteran workspace ownership checks.
[ ] Admin tools do not silently alter generated claim content.
[ ] Admin write actions are auditable.
[ ] Request body content is not logged in admin audit events.

## Build checks

[ ] Backend build passes.
[ ] Web build passes.
[ ] Cloud Run backend deploy succeeds.
[ ] Web app works against Cloud Run backend.

## Close-out

[ ] Build admin roles and access control = 100%.
[ ] Build admin dashboard shell and navigation = 100%.
[ ] Build source metadata manager = 100%.
[ ] Build question and document template editor = 100%.
[ ] Build prompt and disclaimer versioning = 100%.
[ ] Build knowledge base audit review view = 100%.
[ ] Add admin audit logging = 100%.
[ ] Add admin smoke test checklist = 100%.
