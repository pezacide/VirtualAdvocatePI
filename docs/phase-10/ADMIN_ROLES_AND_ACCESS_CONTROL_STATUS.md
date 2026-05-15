# Admin Roles and Access Control Status

## App

Virtual Advocate PI

## Phase

Phase 10 - Admin knowledge and template manager

## Task

Build admin roles and access control

## Status

Completed.

## Existing model confirmed

AppUser already has a Role field.

VirtualAdvocateDbContext already maps Role as a required string field.

No migration was required for the first admin access-control pass.

## Backend files created

backend/src/VirtualAdvocatePI.Api/Services/AdminAccessService.cs.

backend/src/VirtualAdvocatePI.Api/Features/Admin/AdminAccessEndpoints.cs.

## Backend files updated

backend/src/VirtualAdvocatePI.Api/Services/CurrentUserService.cs.

backend/src/VirtualAdvocatePI.Api/Program.cs.

## Frontend files created

web/src/lib/api/admin.ts.

web/src/components/AdminAccessCheckPanel.tsx.

web/src/app/admin/access-check/page.tsx.

## Frontend files updated

web/src/lib/api/index.ts.

## Behaviour

New users still default to VETERAN unless their email is configured as an admin bootstrap email.

Configured admin emails are promoted to ADMIN on login.

Existing ADMIN or SUPER_ADMIN users remain admins.

Protected admin endpoints return 403 for non-admin users.

Admin status can be checked at /admin/access-check.

## Environment variables

VAPI_ADMIN_EMAILS.

ADMIN_EMAILS.

## Admin endpoints

GET /api/v1/admin/me.

GET /api/v1/admin/ping.

## Safety boundary

Admin tools must not be available to normal users.

Admin actions must be auditable in later Phase 10 tasks.

Admin tools must not bypass veteran workspace ownership checks.

Admin tools must not silently alter generated claim content.

## Next task

Build admin dashboard shell and navigation.
